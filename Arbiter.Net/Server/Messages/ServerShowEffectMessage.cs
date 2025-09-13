using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ShowEffect)]
public class ServerShowEffectMessage : ServerMessage
{
    public uint TargetId { get; set; }
    public ushort TargetAnimation { get; set; }
    
    public ushort? TargetX { get; set; }
    public ushort? TargetY { get; set; }
    
    public uint? SourceId { get; set; }
    public ushort? SourceAnimation { get; set; }
    
    public ushort AnimationSpeed { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        TargetId = reader.ReadUInt32();

        if (TargetId == 0)
        {
            TargetAnimation = reader.ReadUInt16();
            AnimationSpeed = reader.ReadUInt16();
            TargetX = reader.ReadUInt16();
            TargetY = reader.ReadUInt16();
        }
        else
        {
            SourceId = reader.ReadUInt32();
            TargetAnimation = reader.ReadUInt16();
            SourceAnimation = reader.ReadUInt16();
            AnimationSpeed = reader.ReadUInt16();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}