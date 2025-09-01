using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.PlaySound)]
public class ServerPlaySoundMessage :IPacketMessage
{
    [InspectSection("Sound", IsExpandedHandler = nameof(ShouldShowSound))]
    [InspectProperty]
    public byte? Effect { get; set; }
    
    [InspectSection("Music", IsExpandedHandler = nameof(ShouldShowMusic))]
    [InspectProperty]
    public byte? Track { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public ushort? Unknown { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
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
    
    private bool ShouldShowSound() => Effect.HasValue;
    private bool ShouldShowMusic() => Track.HasValue;
}