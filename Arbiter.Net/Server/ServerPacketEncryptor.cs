using Arbiter.Net.Security;

namespace Arbiter.Net.Server;

public class ServerPacketEncryptor : INetworkPacketEncryptor
{
    public NetworkEncryptionParameters Parameters { get; set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command) => command is not 0x00 and not 0x03 and not 0x6F and not 0x7E;
    
    public NetworkPacket Encrypt(NetworkPacket packet, byte sequence)
    {
        return packet;
    }

    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        return packet;
    }

    private static bool UseDefaultKey(byte command) =>
        command is 0x01 or 0x02 or 0x0A or 0x56 or 0x60 or 0x62 or 0x66 or 0x6F;
}