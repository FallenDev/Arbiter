using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.RemoveItem)]
public class ServerRemoveItemMessage : IPacketMessage
{
    [InspectSection("Item")]
    [InspectProperty]
    public byte Slot { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
    }
}