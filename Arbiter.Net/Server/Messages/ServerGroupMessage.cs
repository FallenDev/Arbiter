using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.Group)]
public class ServerGroupMessage : ServerMessage
{
    public ServerGroupAction Action { get; set; }
    public string Name { get; set; } = string.Empty;
    public ServerGroupBox? GroupBox { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Action = (ServerGroupAction)reader.ReadByte();
        
        if (Action == ServerGroupAction.RecruitInfo)
        {
            // Yes the ordering of this is different from the client group box
            GroupBox = new ServerGroupBox
            {
                Leader = reader.ReadString8(),
                Name = reader.ReadString8(),
                Note = reader.ReadString8(),
                MinLevel = reader.ReadByte(),
                MaxLevel = reader.ReadByte(),
                MaxWarriors = reader.ReadByte(),
                CurrentWarriors = reader.ReadByte(),
                MaxWizards = reader.ReadByte(),
                CurrentWizards = reader.ReadByte(),
                MaxMonks = reader.ReadByte(),
                CurrentRogues = reader.ReadByte(),
                MaxPriests = reader.ReadByte(),
                CurrentPriests = reader.ReadByte(),
                MaxRogues = reader.ReadByte(),
                CurrentMonks = reader.ReadByte(),
            };

            Name = GroupBox.Leader;
        }
        else
        {
            Name = reader.ReadString8();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}