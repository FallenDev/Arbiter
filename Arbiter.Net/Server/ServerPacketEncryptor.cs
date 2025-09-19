using Arbiter.Net.Security;

namespace Arbiter.Net.Server;

public class ServerPacketEncryptor : INetworkPacketEncryptor
{
    private const int MaxStackAllocLimit = 1024;

    public NetworkEncryptionParameters Parameters { get; set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command) => command is not 0x00 and not 0x03 and not 0x40 and not 0x7E;

    private static bool UseStaticKey(byte command) =>
        command is 0x01 or 0x02 or 0x0A or 0x56 or 0x60 or 0x62 or 0x66 or 0x6F;

    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        if (!IsEncrypted(packet.Command))
        {
            return packet;
        }

        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;
        var keyLength = parameters.PrivateKey.Length;

        // [u8 Sequence] [u8... Payload] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        var payloadLength = packet.Data.Length - 4;
        var payload = new Span<byte>(packet.Data, 1, payloadLength);

        var sequence = packet.Data[0];

        // Some packets use the static fixed key
        // Others use the newer MD5 key table slice
        Span<byte> privateKey = stackalloc byte[keyLength];
        if (UseStaticKey(packet.Command))
        {
            parameters.PrivateKey.Span.CopyTo(privateKey);
        }
        else
        {
            // Extract the trailing random values
            var (sRand, bRand) = ReadRandoms(packet.Data);
            parameters.GenerateKey(bRand, sRand, privateKey);
        }

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

        return new ServerPacket(packet.Command, decrypted) { Sequence = sequence };
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
        var saltTable = parameters.SaltTable.Span;
        var keyLength = parameters.PrivateKey.Length;

        // Some packets use the static fixed key
        // Others use the newer MD5 key table slice
        Span<byte> privateKey = stackalloc byte[keyLength];
        if (UseStaticKey(packet.Command))
        {
            parameters.PrivateKey.Span.CopyTo(privateKey);
        }
        else
        {
            parameters.GenerateKey(bRand, sRand, privateKey);
        }

        // [u8 Sequence] [u8... Payload] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        var totalLength = packet.Data.Length + 4;
        var encrypted = totalLength <= MaxStackAllocLimit ? stackalloc byte[totalLength] : new byte[totalLength];

        // Copy the sequence and unencrypted payload
        encrypted[0] = sequence;
        packet.Data.CopyTo(encrypted[1..]);

        for (var i = 0; i < packet.Data.Length; i++)
        {
            encrypted[i + 1] ^= privateKey[i % keyLength];
            encrypted[i + 1] ^= saltTable[i / keyLength % saltTable.Length];

            if (i / keyLength % saltTable.Length != sequence)
            {
                encrypted[i + 1] ^= saltTable[sequence];
            }
        }

        // Finally include the obfuscated sRand and bRand values
        WriteRandoms(encrypted, sRand, bRand);
        return new ServerPacket(packet.Command, encrypted);
    }

    public static (byte sRand, ushort bRand) ReadRandoms(ReadOnlySpan<byte> buffer)
    {
        var sRand = (byte)(buffer[^2] ^ 0x24);
        var bRand = (ushort)((buffer[^1] << 8 | buffer[^3]) ^ 0x6474);
        return (sRand, bRand);
    }

    public static void WriteRandoms(Span<byte> buffer, byte sRand, ushort bRand)
    {
        buffer[^3] = (byte)((bRand & 0xFF) ^ 0x74);
        buffer[^2] = (byte)(sRand ^ 0x24);
        buffer[^1] = (byte)(((bRand >> 8) & 0xFF) ^ 0x64);
    }
}