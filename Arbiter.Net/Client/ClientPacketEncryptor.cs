using System.Security.Cryptography;
using Arbiter.Net.Security;

namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : INetworkPacketEncryptor
{
    private const int MaxStackAllocLimit = 1024;
    private readonly Crc16Provider _crc16Provider = new();
    private readonly Crc32Provider _crc32Provider = new();
    
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

        // Extract the packet checksum
        var checksum = (uint)packet.Data[^7] << 24 | (uint)packet.Data[^6] << 16 | (uint)packet.Data[^5] << 8 |
                       packet.Data[^4];

        // [u8 Sequence] [DialogHeader?] [u8... Payload] [u8? Command] [u32 Checksum] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        // DialogHeader: [u8 xPrime] [u8 x] [u16 Length] [u16 Checksum]
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
            var (sRand, bRand) = ReadRandoms(packet.Data);
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

        if (UseStaticKey(packet.Command))
        {
            return EncryptWithStaticKey(packet, sequence, sRand, bRand);
        }

        if (!IsDialog(packet.Command))
        {
            return EncryptWithHashKey(packet, sequence, sRand, bRand);
        }
        
        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;
        var keyLength = parameters.PrivateKey.Length;

        // Determine if this packet is a dialog packet, if so, it will have an additional header before the payload
        var isDialog = IsDialog(packet.Command);
        var usesHashKey = !UseStaticKey(packet.Command);
        
        // [u8 Sequence] [DialogHeader?] [u8... Payload] [u8? Command] [u32 Checksum] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        // DialogHeader: [u8 xPrime] [u8 x] [u16 Length] [u16 Checksum]
        var payloadStart = isDialog ? 7 : 1;
        
        // The payload length will also include the command byte if we are using the hash key
        var totalLength = packet.Data.Length + (usesHashKey ? 1 : 0) + (isDialog ? 6 : 0) + 8;
        
        // Some packets use the static fixed key
        // Others use the newer MD5 key table slice
        Span<byte> privateKey = stackalloc byte[keyLength];
        if (usesHashKey)
        {
            parameters.GenerateKey(bRand, sRand, privateKey);
        }
        else
        {
            parameters.PrivateKey.Span.CopyTo(privateKey);
        }

        // Allocate a buffer for the entire encrypted payload, including headers and trailing random values
        var encrypted = totalLength <= MaxStackAllocLimit ? stackalloc byte[totalLength] : new byte[totalLength];

        // Copy the sequence first
        encrypted[0] = sequence;

        if (isDialog)
        {
            // Generate the dialog header and write before the payload
            // This length should not include the trailing command byte
            var dataLengthPlusTwo = packet.Data.Length + 2;
            var checksum = _crc16Provider.Compute(packet.Data);

            encrypted[1] = (byte)((Random.Shared.Next() & 0xFF) + 1);
            encrypted[2] = (byte)((Random.Shared.Next() & 0xFF) + 1);
            encrypted[3] = (byte)(dataLengthPlusTwo >> 8);
            encrypted[4] = (byte)(dataLengthPlusTwo & 0xFF);
            encrypted[5] = (byte)(checksum >> 8);
            encrypted[6] = (byte)(checksum & 0xFF);

            // Copy the payload after the dialog header
            packet.Data.CopyTo(encrypted[payloadStart..]);

            // Generate the magic values
            var xPrime = (byte)(encrypted[1] - 0x2D);
            var x = (byte)(encrypted[2] ^ xPrime);
            var y = (byte)(x + 0x72);
            var z = (byte)(x + 0x28);
            encrypted[3] ^= y;
            encrypted[4] ^= (byte)((y + 1) & 0xFF);

            // Perform the dialog encryption
            for (var i = 0; i < packet.Data.Length; i++)
            {
                encrypted[5 + i] ^= (byte)((z + i) & 0xFF);
            }
        }
        else
        {
            // Copy the payload
            packet.Data.CopyTo(encrypted[payloadStart..]);
        }

        // If we are not using the static key, then we need to add the command to the end of the payload
        if (usesHashKey)
        {
            encrypted[^8] = packet.Command;
        }
        
        // Perform the standard encryption on the current payload
        var length = packet.Data.Length + (usesHashKey ? 1 : 0);
        for (var i = 0; i < length; i++)
        {
            encrypted[payloadStart + i] ^= privateKey[i % keyLength];
            encrypted[payloadStart + i] ^= saltTable[i / keyLength % saltTable.Length];

            if (i / keyLength % saltTable.Length != sequence)
            {
                encrypted[payloadStart + i] ^= saltTable[sequence];
            }
        }

        return packet;
    }

    private ClientPacket EncryptWithStaticKey(NetworkPacket packet, byte sequence, byte sRand, ushort bRand)
    {
        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;

        var keyLength = parameters.PrivateKey.Length;
        Span<byte> privateKey = stackalloc byte[keyLength];
        parameters.PrivateKey.Span.CopyTo(privateKey);

        // [u8 Sequence] [u8... Payload] [u32 Checksum] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        var totalLength = packet.Data.Length + 9;
        var buffer = totalLength <= MaxStackAllocLimit ? stackalloc byte[totalLength] : new byte[totalLength];

        // Copy the command and sequence first, this is needed to generate the checksum
        buffer[0] = packet.Command;
        buffer[1] = sequence;

        packet.Data.CopyTo(buffer[2..]);

        // Encrypt the payload
        for (var i = 0; i < packet.Data.Length; i++)
        {
            buffer[2 + i] ^= privateKey[i % keyLength];
            buffer[2 + i] ^= saltTable[i / keyLength % saltTable.Length];

            if (i / keyLength % saltTable.Length != sequence)
            {
                buffer[2 + i] ^= saltTable[sequence];
            }
        }

        // Determine the MD5 checksum of the [Command] [Sequence] [Payload]
        Span<byte> checksum = stackalloc byte[16];
        MD5.HashData(buffer[..^7], checksum);

        buffer[^7] = checksum[13];
        buffer[^6] = checksum[3];
        buffer[^5] = checksum[11];
        buffer[^4] = checksum[7];

        WriteRandoms(buffer, sRand, bRand);
        return new ClientPacket(buffer[0], buffer[1..]);
    }
    
    private ClientPacket EncryptWithHashKey(NetworkPacket packet, byte sequence, byte sRand, ushort bRand)
    {
        var parameters = Parameters;
        var saltTable = parameters.SaltTable.Span;

        var keyLength = parameters.PrivateKey.Length;
        Span<byte> privateKey = stackalloc byte[keyLength];
        parameters.GenerateKey(bRand, sRand, privateKey);

        // [u8 Sequence] [u8... Payload] [u8 Command] [u32 Checksum] [u8 bRand Lo] [u8 sRand] [u8 bRand Hi]
        var totalLength = packet.Data.Length + 10;
        var buffer = totalLength <= MaxStackAllocLimit ? stackalloc byte[totalLength] : new byte[totalLength];

        // Copy the command and sequence first, this is needed to generate the checksum
        buffer[0] = packet.Command;
        buffer[1] = sequence;

        packet.Data.CopyTo(buffer[2..]);
        buffer[packet.Data.Length + 2] = packet.Command; 

        // Encrypt the payload
        for (var i = 0; i < packet.Data.Length + 1; i++)
        {
            buffer[2 + i] ^= privateKey[i % keyLength];
            buffer[2 + i] ^= saltTable[i / keyLength % saltTable.Length];

            if (i / keyLength % saltTable.Length != sequence)
            {
                buffer[2 + i] ^= saltTable[sequence];
            }
        }

        // Determine the MD5 checksum of the [Command] [Sequence] [Payload] [Command]
        Span<byte> checksum = stackalloc byte[16];
        MD5.HashData(buffer[..^7], checksum);

        buffer[^7] = checksum[13];
        buffer[^6] = checksum[3];
        buffer[^5] = checksum[11];
        buffer[^4] = checksum[7];

        WriteRandoms(buffer, sRand, bRand);
        return new ClientPacket(buffer[0], buffer[1..]);
    }

    public static (byte sRand, ushort bRand) ReadRandoms(ReadOnlySpan<byte> buffer)
    {
        var sRand = (byte)(buffer[^2] ^ 0x23);
        var bRand = (ushort)((buffer[^1] << 8 | buffer[^3]) ^ 0x7470);
        return (sRand, bRand);
    }

    public static void WriteRandoms(Span<byte> buffer, byte sRand, ushort bRand)
    {
        buffer[^3] = (byte)((bRand & 0xFF) ^ 0x70);
        buffer[^2] = (byte)(sRand ^ 0x23);
        buffer[^1] = (byte)(((bRand >> 8) & 0xFF) ^ 0x74);
    }
}