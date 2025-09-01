using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapChanging)]
public class ServerMapChangingMessage : IPacketMessage
{
    [InspectSection("Parameters")]
    [InspectProperty]
    public byte ChangeType { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public uint Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        ChangeType = reader.ReadByte();
        Unknown = reader.ReadUInt32();
    }
}