using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.SwapSlot)]
public class ClientSwapSlotMessage : ClientMessage
{
    public InterfacePane Pane { get; set; }
    public byte SourceSlot { get; set; }
    public byte TargetSlot { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Pane = (InterfacePane)reader.ReadByte();
        SourceSlot = reader.ReadByte();
        TargetSlot = reader.ReadByte();
    }
    
    public override void Serialize(ref NetworkPacketBuilder builder)
    {
        base.Serialize(ref builder);
        
        builder.AppendByte((byte)Pane);
        builder.AppendByte(SourceSlot);
        builder.AppendByte(TargetSlot);
    }
}