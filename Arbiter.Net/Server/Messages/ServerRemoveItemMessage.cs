using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.RemoveItem)]
public class ServerRemoveItemMessage : ServerMessage
{
    public byte Slot { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(Slot);
    }
}