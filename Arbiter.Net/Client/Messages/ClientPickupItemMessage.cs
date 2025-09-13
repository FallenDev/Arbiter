using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.PickupItem)]
public class ClientPickupItemMessage : ClientMessage
{
    public byte Slot { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}