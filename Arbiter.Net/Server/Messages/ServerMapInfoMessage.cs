using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerMapInfoMessage : ServerMessage
{
    public ushort MapId { get; set; }
    public MapWeatherFlags Weather { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort Checksum { get; set; }
    public string Name { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MapId = reader.ReadUInt16();

        var widthLo = reader.ReadByte();
        var heightLo = reader.ReadByte();

        Weather = (MapWeatherFlags)reader.ReadByte();

        var widthHi = reader.ReadByte();
        var heightHi = reader.ReadByte();

        Width = (ushort)(widthHi << 8 | widthLo);
        Height = (ushort)(heightHi << 8 | heightLo);

        // These are swapped with the normal "big endian" order
        var checksumLo = reader.ReadByte();
        var checksumHi = reader.ReadByte();
        Checksum = (ushort)(checksumHi << 8 | checksumLo);

        Name = reader.ReadString8();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}