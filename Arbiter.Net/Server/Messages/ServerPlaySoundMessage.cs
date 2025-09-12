using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerPlaySoundMessage : ServerMessage
{
    public byte? Effect { get; set; }
    public byte? Track { get; set; }
    public ushort? Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        var sound = reader.ReadByte();
        if (sound == 0xFF)
        {
            Track = reader.ReadByte();
            Unknown = reader.ReadUInt16();
        }
        else
        {
            Effect = sound;
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }

    // Migrate these
    private bool ShouldShowSound() => Effect.HasValue;
    private bool ShouldShowMusic() => Track.HasValue;
}