using System.Reflection;
using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMessageFactory : IServerMessageFactory
{
    public static IServerMessageFactory Default { get; } = new ServerMessageFactory();
    
    private readonly Dictionary<ServerCommand, Type> _typeMappings = new();

    public ServerMessageFactory()
    {
        RegisterAnnotatedMessages();
    }
    
    private void RegisterAnnotatedMessages()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IServerMessage).IsAssignableFrom(t));

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<NetworkCommandAttribute>(inherit: true);
            if (attr is null)
            {
                continue;
            }

            var command = (ServerCommand)attr.Command;
            _typeMappings.Add(command, type);
        }
    }
    
    public Type? GetMessageType(ServerCommand command) => _typeMappings.GetValueOrDefault(command);

    public ServerCommand? GetMessageCommand(Type messageType)
    {
        foreach (var (key, value) in _typeMappings)
        {
            if (value == messageType)
            {
                return key;
            }
        }

        return null;
    }

    public IServerMessage? Create(ServerPacket packet)
    {
        var type = GetMessageType(packet.Command);
        if (type is null)
        {
            return null;
        }

        var instance = (IServerMessage)Activator.CreateInstance(type)!;
        var reader = new NetworkPacketReader(packet);
        instance.Deserialize(reader);

        return instance;
    }
}