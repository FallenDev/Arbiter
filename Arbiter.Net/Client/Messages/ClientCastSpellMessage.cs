using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.CastSpell)]
public class ClientCastSpellMessage : ClientMessage
{
    public byte Slot { get; set; }
    public uint? TargetId { get; set; }
    public ushort? TargetX { get; set; }
    public ushort? TargetY { get; set; }
    public string? TextInput { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();

        var maybeLength = reader.ReadByte();

        if (maybeLength == 0x00 && reader.CanRead(8))
        {
            reader.Position -= 1;
            
            TargetId = reader.ReadUInt32();
            TargetX = reader.ReadUInt16();
            TargetY = reader.ReadByte();
        }

        if (maybeLength != 0x00 && reader.CanRead(maybeLength))
        {
            TextInput = reader.ReadFixedString(maybeLength);
        }
    }
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}