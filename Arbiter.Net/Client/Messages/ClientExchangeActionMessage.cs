using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.ExchangeAction)]
public class ClientExchangeActionMessage : ClientMessage
{
    public ExchangeClientActionType Action { get; set; }
    public uint TargetId { get; set; }
    public byte? Slot { get; set; }
    public byte? Quantity { get; set; }
    public uint? GoldAmount { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Action = (ExchangeClientActionType)reader.ReadByte();
        TargetId = reader.ReadUInt32();

        if (Action is ExchangeClientActionType.AddItem or ExchangeClientActionType.AddStackableItem)
        {
            Slot = reader.ReadByte();
            Quantity = Action == ExchangeClientActionType.AddStackableItem ? reader.ReadByte() : (byte)1;
        }
        else if (Action == ExchangeClientActionType.SetGold)
        {
            GoldAmount = reader.ReadUInt32();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}