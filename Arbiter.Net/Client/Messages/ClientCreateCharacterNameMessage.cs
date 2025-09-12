using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientCreateCharacterNameMessage : INetworkSerializable
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        Name = reader.ReadString8();
        Password = reader.ReadString8();
        Email = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}