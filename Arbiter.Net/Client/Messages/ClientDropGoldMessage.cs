using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.DropGold)]
public class ClientDropGoldMessage : ClientMessage
{
    public uint Amount { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Amount = reader.ReadUInt32();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}