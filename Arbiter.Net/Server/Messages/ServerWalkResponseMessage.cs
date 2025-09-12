using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerWalkResponseMessage : INetworkSerializable
{
    public WorldDirection Direction { get; set; }
    public ushort PreviousX { get; set; }
    public ushort PreviousY { get; set; }
    public ushort UnknownX { get; set; }
    public ushort UnknownY { get; set; }
    public byte Unknown { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Direction = (WorldDirection)reader.ReadByte();
        PreviousX = reader.ReadUInt16();
        PreviousY = reader.ReadUInt16();

        // not sure what these values are for, visible range?
        UnknownX = reader.ReadUInt16();
        UnknownY = reader.ReadUInt16();
        Unknown = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}