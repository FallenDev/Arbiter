using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.AddItem)]
public class ServerAddItemMessage : IPacketMessage
{
    [InspectSection("Item")]
    [InspectProperty]
    public byte Slot { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort Icon { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string Name { get; set; } = string.Empty;
    
    [InspectProperty]
    public uint Quantity { get; set; }
    
    [InspectSection("Traits")]
    [InspectProperty]
    public byte Color { get; set; }
    
    [InspectProperty]
    public bool IsStackable { get; set; }
    
    [InspectSection("Durability")]
    [InspectProperty]
    public uint Durability { get; set; }
    
    [InspectProperty]
    public uint MaxDurability { get; set; }
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        Color = reader.ReadByte();
        Name = reader.ReadString8();
        Quantity = reader.ReadUInt32();
        IsStackable = reader.ReadBoolean();
        MaxDurability = reader.ReadUInt32();
        Durability = reader.ReadUInt32();
    }
}