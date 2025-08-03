using Arbiter.Net.Security;

namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : INetworkPacketEncryptor
{
    public NetworkEncryptionParameters Parameters { get; set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command) => command is not 0x00 and not 0x0B and not 0x10 and not 0x48 and not 0x57 and not 0x62;
    
    public NetworkPacket Encrypt(NetworkPacket packet)
    {
        var parameters = Parameters;
        
        return packet;
    }

    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;
        var keyLength = parameters.PrivateKey.Length;

        // [u8 Sequence] [u8... Payload] [u8 sRand] [u16 bRand]
        var payloadLength = packet.Data.Length - 4;
        var payload = new Span<byte>(packet.Data, 1, payloadLength);

        // Extract the relevant encryption values
        var sequence = packet.Data[0];
        var sRand = (byte)(packet.Data[^3] ^ 0x23);
        var bRand = (ushort)((packet.Data[^2] << 8 | packet.Data[^1]) ^ 0x7470);

        // Some packets use the default fixed key
        // Others use the newer MD5 key table slice
        Span<byte> privateKey = stackalloc byte[keyLength];
        if (UseDefaultKey(packet.Command))
        {
            parameters.PrivateKey.Span.CopyTo(privateKey);
        }
        else
        {
            parameters.GenerateKey(bRand, sRand, privateKey);
        }

        var decrypted = new byte[payloadLength];
        payload.CopyTo(decrypted);

        // Decrypt the payload
        for (var i = 0; i < payloadLength; i++)
        {
            decrypted[i] ^= privateKey[i % keyLength];
            decrypted[i] ^= saltTable[i / keyLength % saltTable.Length];

            if (i / keyLength % saltTable.Length != sequence)
            {
                decrypted[i] ^= saltTable[sequence];
            }
        }

        var decryptedPacket =  new ClientPacket(packet.Command, decrypted);
        return decryptedPacket;
    }


    private static bool UseDefaultKey(byte command) => command is 0x02 or 0x03 or 0x04 or 0x0B or 0x26 or 0x2D or 0x3A or 0x42
        or 0x43 or 0x4B or 0x57 or 0x62 or 0x68 or 0x71 or 0x73 or 0x7B;

    private static bool IsDialog(byte command) => command is 0x39 or 0x3A;
}