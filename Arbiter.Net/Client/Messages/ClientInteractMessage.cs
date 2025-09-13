using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

public class ClientInteractMessage : ClientMessage
{
    public InteractionType InteractionType { get; set; }
    public uint? TargetId { get; set; }
    public ushort? TargetX { get; set; }
    public ushort? TargetY { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        InteractionType = (InteractionType)reader.ReadByte();

        if (InteractionType == InteractionType.Entity)
        {
            TargetId = reader.ReadUInt32();
        }
        else if (InteractionType == InteractionType.Tile)
        {
            TargetX = reader.ReadUInt16();
            TargetY = reader.ReadUInt16();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}