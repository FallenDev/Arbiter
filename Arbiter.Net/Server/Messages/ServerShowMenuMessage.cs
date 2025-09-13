using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ShowMenu)]
public class ServerShowMenuMessage : ServerMessage
{
    public DialogMenuType MenuType { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        MenuType = (DialogMenuType)reader.ReadByte();
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}