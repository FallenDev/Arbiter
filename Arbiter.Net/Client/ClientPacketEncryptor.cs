namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : NetworkPacketEncryptor
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