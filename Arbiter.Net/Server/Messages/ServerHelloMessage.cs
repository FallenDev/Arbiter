using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerHelloMessage : ServerMessage
{
    public string Message { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        reader.Skip(1);
        Message = reader.ReadLine();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}