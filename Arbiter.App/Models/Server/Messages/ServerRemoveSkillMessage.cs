using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.RemoveSkill)]
public class ServerRemoveSkillMessage : IPacketMessage
{
    [InspectSection("Skill")]
    [InspectProperty]
    public byte Slot { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
    }
}