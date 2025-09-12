using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientChangePasswordMessage : INetworkSerializable
{
    public string Name { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        Name = reader.ReadString8();
        CurrentPassword = reader.ReadString8();
        NewPassword = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}