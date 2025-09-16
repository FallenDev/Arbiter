using Arbiter.Net.Annotations;
using Arbiter.Net.Client;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ForcePacket)]
public class ServerForcePacketMessage : ServerMessage
{
    public ClientCommand ClientCommand { get; set; }
    public IReadOnlyList<byte> Data { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        var length = reader.ReadUInt16();
        ClientCommand = (ClientCommand) reader.ReadByte();
        Data = reader.ReadBytes(length - 1);
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}