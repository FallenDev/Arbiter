using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Net.Client.Messages;

public interface IClientMessageFactory
{
    Type? GetMessageType(ClientCommand command);
    ClientCommand? GetMessageCommand(Type messageType);
    
    IClientMessage? Create(ClientPacket packet);
    
    bool TryCreate(ClientPacket packet, [NotNullWhen(true)] out IClientMessage? message)
    {
        try
        {
            message = Create(packet);
            return message is not null;
        }
        catch
        {
            message = null;
            return false;
        }
    }
}