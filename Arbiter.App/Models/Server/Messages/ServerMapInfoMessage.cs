using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapInfo)]
public class ServerMapInfoMessage : IPacketMessage
{
    [InspectSection("Information")]
    [InspectProperty]
    public ushort MapId { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Name { get; set; } = string.Empty;
    
    [InspectProperty(ShowHex = true)]
    public ushort Checksum { get; set; }
    
    [InspectSection("Dimensions")]
    [InspectProperty]
    public ushort Width { get; set; }
    
    [InspectProperty]
    public ushort Height { get; set; }
    
    [InspectSection("Flags")]
    [InspectProperty]
    public MapWeatherFlags Weather { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
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
}