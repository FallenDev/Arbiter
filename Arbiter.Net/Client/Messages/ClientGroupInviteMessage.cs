using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.GroupInvite)]
public class ClientGroupInviteMessage : ClientMessage
{
    public GroupInviteAction GroupAction { get; set; }
    public string TargetName { get; set; } = string.Empty;
    
    public ClientGroupBox? GroupBox { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        GroupAction = (GroupInviteAction)reader.ReadByte();
        TargetName = reader.ReadString8();

        if (GroupAction == GroupInviteAction.CreateGroupBox)
        {
            GroupBox = new ClientGroupBox
            {
                Name = reader.ReadString8(),
                Note = reader.ReadString8(),
                MinLevel = reader.ReadByte(),
                MaxLevel = reader.ReadByte(),
                MaxWarriors = reader.ReadByte(),
                MaxWizards = reader.ReadByte(),
                MaxRogues = reader.ReadByte(),
                MaxPriests = reader.ReadByte(),
                MaxMonks = reader.ReadByte()
            };
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}