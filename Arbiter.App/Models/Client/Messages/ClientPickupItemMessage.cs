using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.PickupItem)]
public class ClientPickupItemMessage : IPacketMessage
{
    [InspectSection("Inventory")]
    [InspectProperty]
    public byte Slot { get; set; }
    
    [InspectSection("Location")]
    [InspectProperty]
    public ushort X { get; set; }
    
    [InspectProperty]
    public ushort Y { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
    }
}