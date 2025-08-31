using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.App.Models;

public class PacketMessageFactory
{
    private readonly Dictionary<byte, Func<NetworkPacketReader, object>> _clientPacketMap = new();
    private readonly Dictionary<byte, Func<NetworkPacketReader, object>> _serverPacketMap = new();

    public void RegisterFromAssembly()
    {
        var packetTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPacketMessage).IsAssignableFrom(t));

        foreach (var type in packetTypes)
        {
            var attr = type.GetCustomAttribute<InspectPacketAttribute>();
            if (attr is null || attr.Direction != PacketDirection.Client && attr.Direction != PacketDirection.Server)
            {
                continue;
            }

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new InvalidOperationException($"{type.Name} does not have a parameterless constructor");
            }

            var ctorDelegate = CompileConstructor(type, ctor);
            var factory = BuildFactoryDelegate(type, ctorDelegate);

            if (attr.Direction == PacketDirection.Client)
            {
                if (!_clientPacketMap.TryAdd(attr.Command, factory))
                {
                    throw new InvalidOperationException($"Duplicate packet client command: 0x{attr.Command}");
                }
            }
            else if (attr.Direction == PacketDirection.Server)
            {
                if (!_serverPacketMap.TryAdd(attr.Command, factory))
                {
                    throw new InvalidOperationException($"Duplicate packet server command: 0x{attr.Command}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown packet direction: {attr.Direction}");
            }
        }
    }

    public bool CanParse(NetworkPacket packet) => packet switch
    {
        ClientPacket => _clientPacketMap.ContainsKey(packet.Command),
        ServerPacket => _serverPacketMap.ContainsKey(packet.Command),
        _ => false
    };

    public bool TryParsePacket(NetworkPacket packet, [NotNullWhen(true)] out IPacketMessage? message,
        [NotNullWhen(false)] out Exception? exception)
    {
        message = null;
        exception = null;

        try
        {
            var factory = packet switch
            {
                ClientPacket => _clientPacketMap.GetValueOrDefault(packet.Command),
                ServerPacket => _serverPacketMap.GetValueOrDefault(packet.Command),
                _ => null
            };

            if (factory is null)
            {
                var direction = packet is ClientPacket ? "client" : "server";
                exception = new InvalidOperationException($"Unknown {direction} packet command: 0x{packet.Command:X2}");
                return false;
            }

            var reader = new NetworkPacketReader(packet);
            var parsed = factory(reader) as IPacketMessage;

            message = parsed;
            return message is not null;
        }
        catch (Exception ex)
        {
            exception = ex;
            message = null;
            return false;
        }
    }

    private static Func<object> CompileConstructor(Type type, ConstructorInfo ctor)
    {
        var newExpression = Expression.New(ctor);
        var cast = Expression.Convert(newExpression, type);
        var lambda = Expression.Lambda<Func<object>>(cast);
        return lambda.Compile();
    }

    private static Func<NetworkPacketReader, object> BuildFactoryDelegate<T>(Type type, Func<T> constructor)
    {
        return reader =>
        {
            var obj = constructor();
            if (obj is not IPacketMessage message)
            {
                throw new InvalidOperationException($"{type.Name} does not implement {nameof(IPacketMessage)}");
            }

            message.ReadFrom(reader);
            return message;
        };
    }
}