using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Login)]
public class ClientLoginMessage : IPacketMessage
{
    [InspectSection("Credentials")]
    [InspectProperty]
    public string Name { get; set; } = string.Empty;
    
    [InspectProperty]
    [InspectMasked]
    public string Password { get; set; } = string.Empty;
    
    [InspectSection("Client")]
    [InspectProperty(ShowHex = true)]
    public uint ClientId { get; set; }
    [InspectProperty(ShowHex = true)]
    public ushort Checksum { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Name = reader.ReadString8();
        Password = reader.ReadString8();

        var key1 = reader.ReadByte();
        var key2 = reader.ReadByte();

        var encodedClientId = reader.ReadUInt32();
        var encodedChecksum = reader.ReadUInt16();

        ClientId = DecodeClientId(encodedClientId, key1, key2);
        Checksum = DecodeChecksum(encodedChecksum, key1, key2);
    }

    private static uint DecodeClientId(uint encoded, byte key1, byte key2)
    {
        var compoundKey = (byte)(key2 ^ (byte)(key1 + 0x3B));
        var clientIdKey = (byte)(compoundKey + 0x8A);
        
        var clientIdMask = clientIdKey | ((uint)(clientIdKey + 1) << 8) | ((uint)(clientIdKey + 2) << 16) |
                           ((uint)(clientIdKey + 3) << 24);

        return encoded ^ clientIdMask;
    }

    private static ushort DecodeChecksum(ushort encoded, byte key1, byte key2)
    {
        var compoundKey = (byte)(key2 ^ (byte)(key1 + 0x3B));
        var checksumKey = (byte)(compoundKey + 0x5E);

        var checkSumMask = checksumKey | ((ushort)(checksumKey + 1) << 8);

        return (ushort)(encoded ^ checkSumMask);
    }
}