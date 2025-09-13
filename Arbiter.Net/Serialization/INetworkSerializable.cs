namespace Arbiter.Net.Serialization;

public interface INetworkSerializable
{
    void Deserialize(INetworkPacketReader reader);
    void Serialize(INetworkPacketBuilder builder);
}