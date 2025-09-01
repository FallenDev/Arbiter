using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.SetEquipment)]
public class ServerSetEquipmentMessage : IPacketMessage
{
    [InspectSection("Equipment")]
    [InspectProperty]
    public EquipmentSlot Slot { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort Sprite { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Name { get; set; } = string.Empty;
    
    [InspectSection("Traits")]
    [InspectProperty]
    public DyeColor DyeColor { get; set; }
    
    [InspectSection("Durability")]
    [InspectProperty]
    public uint Durability { get; set; }
    
    [InspectProperty]
    public uint MaxDurability { get; set; }
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = (EquipmentSlot)reader.ReadByte();
        Sprite = reader.ReadUInt16();
        DyeColor = (DyeColor)reader.ReadByte();
        Name = reader.ReadString8();
        
        reader.Skip(1); // always zero, not sure what it is
        
        MaxDurability = reader.ReadUInt32();
        Durability = reader.ReadUInt32();
    }
}