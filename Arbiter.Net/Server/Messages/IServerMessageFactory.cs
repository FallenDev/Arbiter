using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Net.Server.Messages;

public interface IServerMessageFactory
{
    Type? GetMessageType(ServerCommand command);
    ServerCommand? GetMessageCommand(Type messageType);
    
    IServerMessage? Create(ServerPacket packet);

    bool TryCreate(ServerPacket packet, [NotNullWhen(true)] out IServerMessage? message)
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

    bool TryCreate<T>(ServerPacket packet, [NotNullWhen(true)] out T? message) where T : IServerMessage
    {
        message = default;
        
        if (!TryCreate(packet, out var serverMessage))
        {
            return false;
        }

        if (serverMessage is not T expectedMessage)
        {
            return false;
        }
        message = expectedMessage;
        return true;
    }
}