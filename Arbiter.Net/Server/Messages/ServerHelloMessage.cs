using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerHelloMessage : INetworkSerializable
{
    public string Message { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        reader.Skip(1);
        Message = reader.ReadLine();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}