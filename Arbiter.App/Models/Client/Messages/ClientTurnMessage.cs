using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Turn)]
public class ClientTurnMessage : IPacketMessage
{
    [InspectSection("Turn")]
    [InspectProperty]
    public WorldDirection Direction { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
    }
}