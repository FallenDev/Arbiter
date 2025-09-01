using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.DropItem)]
public class ClientDropItemMessage : IPacketMessage
{

    [InspectSection("Item")]
    [InspectProperty]
    public byte Slot { get; set; }

    [InspectProperty] public uint Quantity { get; set; }

    [InspectSection("Location")]
    [InspectProperty]
    public ushort X { get; set; }

    [InspectProperty] public ushort Y { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        Quantity = reader.ReadUInt32();
    }
}