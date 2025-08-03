namespace Arbiter.Net.Server;

public class ServerPacketEncryptor : NetworkPacketEncryptor
{
    public override NetworkPacket Encrypt(NetworkPacket packet)
    {
        return packet;
    }

    public override NetworkPacket Decrypt(NetworkPacket packet)
    {
        return packet;
    }
}