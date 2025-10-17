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

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();

        if (reader.IsEndOfPacket())
        {
            return;
        }

        var argsLength = reader.ReadByte();
        
        if (argsLength == 0x00 && reader.CanRead(7))
        {
            reader.Position -= 1;
            
            TargetId = reader.ReadUInt32();
            TargetX = reader.ReadUInt16();
            TargetY = reader.ReadByte();
        }

        if (argsLength != 0x00 && reader.CanRead(argsLength))
        {
            TextInput = reader.ReadFixedString(argsLength);
        }
    }
    
    public override void Serialize(ref NetworkPacketBuilder builder)
    {
        base.Serialize(ref builder);
        
        builder.AppendByte(Slot);
        
        if (TargetId.HasValue && TargetX.HasValue && TargetY.HasValue)
        {
            builder.AppendByte(0x00);
            builder.AppendUInt32(TargetId.Value);
            builder.AppendUInt16(TargetX.Value);
            builder.AppendByte((byte)TargetY.Value);
        }
        else if (!string.IsNullOrEmpty(TextInput))
        {
            builder.AppendByte((byte)TextInput.Length);
            builder.AppendString8(TextInput);
        }
        else
        {
            builder.AppendByte(0x00);
        }
    }
}