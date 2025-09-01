using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.ExitResponse)]
public class ServerExitResponseMessage : IPacketMessage
{
    [InspectSection("Response")]
    [InspectProperty]
    public byte Result { get; set; }
    
    [InspectSection("Uncategorized")]
    [InspectProperty(ShowHex = true)]
    public ushort Unknown { get; set; }
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Result = reader.ReadByte();
        Unknown = reader.ReadUInt16();
    }
}