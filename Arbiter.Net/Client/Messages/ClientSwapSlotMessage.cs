using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

public class ClientSwapSlotMessage : ClientMessage
{
    public InterfacePane Pane { get; set; }
    public byte SourceSlot { get; set; }
    public byte TargetSlot { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Pane = (InterfacePane)reader.ReadByte();
        SourceSlot = reader.ReadByte();
        TargetSlot = reader.ReadByte();
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}