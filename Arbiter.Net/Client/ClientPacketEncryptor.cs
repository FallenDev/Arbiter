using Arbiter.Net.Security;

namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : INetworkPacketEncryptor
{
    public NetworkEncryptionParameters Parameters { get; set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command) => command is not 0x00 and not 0x10 and not 0x48;

    private static bool UseStaticKey(byte command) => command is 0x02 or 0x03 or 0x04 or 0x0B or 0x26 or 0x2D or 0x3A
        or 0x42 or 0x43 or 0x4B or 0x57 or 0x62 or 0x68 or 0x71 or 0x73 or 0x7B;

    private static bool IsDialog(byte command) => command is 0x39 or 0x3A;
    
    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        if (!IsEncrypted(packet.Command))
        {
            return packet;
        }
        
        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;
        var keyLength = parameters.PrivateKey.Length;

        // Extract the relevant encryption values
        var sequence = packet.Data[0];
        var sRand = (byte)(packet.Data[^2] ^ 0x23);
        var bRand = (ushort)((packet.Data[^1] << 8 | packet.Data[^3]) ^ 0x7470);

        // Extract the packet checksum
        var checksum = ((uint)packet.Data[^7] << 24 | (uint)packet.Data[^6] << 16 | (uint)packet.Data[^5] << 8 |
                        packet.Data[^4]);

        // [u8 Sequence] [u8... Payload] [u8? Command] [u32 Checksum] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        var payloadLength = packet.Data.Length - 8;

        // Some packets use the static fixed key
        // Others use the newer MD5 key table
        Span<byte> privateKey = stackalloc byte[keyLength];
        if (UseStaticKey(packet.Command))
        {
            parameters.PrivateKey.Span.CopyTo(privateKey);
        }
        else
        {
            parameters.GenerateKey(bRand, sRand, privateKey);
            payloadLength -= 1; // Ignore the duplicated command byte
        }

        var payload = new Span<byte>(packet.Data, 1, payloadLength);
        var decrypted = new byte[payloadLength].AsSpan();
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

        if (!IsDialog(packet.Command))
        {
            return new ClientPacket(packet.Command, decrypted, checksum) { Sequence = sequence };
        }

        // Handle dialog encryption
        // [u8 xPrime] [u8 x] [u16 Length] [u16 Checksum]
        var xPrime = (byte)(decrypted[0] - 0x2D);
        var x = (byte)(decrypted[1] ^ xPrime);
        var y = (byte)(x + 0x72);
        var z = (byte)(x + 0x28);
        decrypted[2] ^= y;
        decrypted[3] ^= (byte)((y + 1) & 0xFF);

        var length = (decrypted[2] << 8) | decrypted[3];
        for (var i = 0; i < length; i++)
        {
            decrypted[4 + i] ^= (byte)((z + i) & 0xFF);
        }

        decrypted = decrypted[6..];

        return new ClientPacket(packet.Command, decrypted, checksum) { Sequence = sequence };
    }
    
    public NetworkPacket Encrypt(NetworkPacket packet, byte sequence)
    {
        // Generate the random values within their expected ranges
        var bRand = (ushort)(Random.Shared.Next(65277) + 256);
        var sRand = (byte)(Random.Shared.Next(155) + 100);

        return Encrypt(packet, sequence, sRand, bRand);
    }
    
    public NetworkPacket Encrypt(NetworkPacket packet, byte sequence, byte sRand, ushort bRand)
    {
        if (!IsEncrypted(packet.Command))
        {
            return packet;
        }
        
        var parameters = Parameters;

        return packet;
    }
}