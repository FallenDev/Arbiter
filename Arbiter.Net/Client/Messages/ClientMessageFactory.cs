using System.Reflection;
using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientMessageFactory : IClientMessageFactory
{
    public static IClientMessageFactory Default { get; } = new ClientMessageFactory();

    private readonly Dictionary<ClientCommand, Type> _typeMappings = new();

    public ClientMessageFactory()
    {
        RegisterAnnotatedMessages();
    }

    private void RegisterAnnotatedMessages()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IClientMessage).IsAssignableFrom(t));
            
        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<NetworkCommandAttribute>();
            if (attr is null)
            {
                continue;
            }

            var command = (ClientCommand)attr.Command;
            _typeMappings.Add(command, type);
        }
    }

    public Type? GetMessageType(ClientCommand command) => _typeMappings.GetValueOrDefault(command);

    public ClientCommand? GetMessageCommand(Type messageType)
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

    public IClientMessage? Create(ClientPacket packet)
    {
        var type = GetMessageType(packet.Command);
        if (type is null)
        {
            return null;
        }

        var instance = (IClientMessage)Activator.CreateInstance(type)!;
        var reader = new NetworkPacketReader(packet);
        instance.Deserialize(reader);

        return instance;
    }
}