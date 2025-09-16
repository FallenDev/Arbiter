using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.Exchange)]
public class ServerExchangeMessage : ServerMessage
{
    public ExchangeServerEventType Event { get; set; }
    public uint? TargetId { get; set; }
    public string? TargetName { get; set; }
    public byte? Slot { get; set; }
    public ExchangeParty? Party { get; set; }
    public byte? ItemIndex { get; set; }
    public ushort? ItemSprite { get; set; }
    public DyeColor? ItemColor { get; set; }
    public string? ItemName { get; set; }
    public uint? GoldAmount { get; set; }
    public string? Message { get; set; }
    
    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Event = (ExchangeServerEventType)reader.ReadByte();

        if (Event == ExchangeServerEventType.Started)
        {
            TargetId = reader.ReadUInt32();
            TargetName = reader.ReadString8();
        }
        else if (Event == ExchangeServerEventType.QuantityPrompt)
        {
            Slot = reader.ReadByte();
        }
        else if (Event == ExchangeServerEventType.ItemAdded)
        {
            Party = (ExchangeParty)reader.ReadByte();
            ItemIndex = reader.ReadByte();
            ItemSprite = reader.ReadUInt16();
            ItemColor = (DyeColor)reader.ReadByte();
            ItemName = reader.ReadString8();
        }
        else if (Event == ExchangeServerEventType.GoldAdded)
        {
            Party = (ExchangeParty)reader.ReadByte();
            GoldAmount = reader.ReadUInt32();
        }
        else if (Event == ExchangeServerEventType.Cancelled)
        {
            Party = (ExchangeParty)reader.ReadByte();
            Message = reader.ReadString8();
        }
        else if (Event == ExchangeServerEventType.Accepted)
        {
            Party = (ExchangeParty)reader.ReadByte();
            Message = reader.ReadString8();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}