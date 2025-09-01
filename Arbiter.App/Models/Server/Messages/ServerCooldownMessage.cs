using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.Cooldown)]
public class ServerCooldownMessage : IPacketMessage
{
    [InspectSection("Cooldown")]
    [InspectProperty]
    public AbilityPane Pane { get; set; }
    
    [InspectProperty]
    public byte Slot { get; set; }
    
    [InspectProperty]
    public uint Seconds { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Pane = (AbilityPane)reader.ReadByte();
        Slot = reader.ReadByte();
        Seconds = reader.ReadUInt32();
    }
}