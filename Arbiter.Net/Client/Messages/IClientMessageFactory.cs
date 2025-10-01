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

    bool TryCreate<T>(ClientPacket packet, [NotNullWhen(true)] out T? message) where T : IClientMessage
    {
        message = default;

        if (!TryCreate(packet, out var clientMessage))
        {
            return false;
        }

        if (clientMessage is not T expectedMessage)
        {
            return false;
        }

        message = expectedMessage;
        return true;
    }
}