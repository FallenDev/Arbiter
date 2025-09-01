using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Walk)]
public class ClientWalkMessage : IPacketMessage
{
    [InspectSection("Walk")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }
    
    [InspectProperty]
    public byte StepCount { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
        StepCount = reader.ReadByte();
    }
}