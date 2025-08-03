using Arbiter.Net.Security;

namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : INetworkPacketEncryptor
{
    // Opcode != 0x00 && Opcode != 0x0B && Opcode != 0x10 && Opcode != 0x48 && Opcode != 0x57 && Opcode != 0x62;
    // 00 = CryptoKey?
    // 0B = UserMove
    
    // Default key = 
    // Opcode is 0x02 or 0x03 or 0x04 or 0x0B or 0x26 or 0x2D or 0x3A or 0x42 or 0x43 or 0x4B or 0x57 or 0x62
    // or 0x68 or 0x71 or 0x73 or 0x7B;
    public NetworkEncryptionParameters Parameters { get; private set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command)
    {
        return true;
    }
    
    public NetworkPacket Encrypt(NetworkPacket packet)
    {
        return packet;
    }

    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        return packet;
    }

    public void SetParameters(NetworkEncryptionParameters parameters)
    {
        Parameters = parameters;
    }
}