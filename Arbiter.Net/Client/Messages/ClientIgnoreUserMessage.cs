using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.IgnoreUser)]
public class ClientIgnoreUserMessage : ClientMessage
{
    public IgnoreUserAction Action { get; set; }
    
    public string? Name { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Action = (IgnoreUserAction)reader.ReadByte();

        if (Action is IgnoreUserAction.AddUser or IgnoreUserAction.RemoveUser)
        {
            Name = reader.ReadString8();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}