using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.SyncTicks)]
public class ClientSyncTicksMessage : ClientMessage
{
    public uint ServerTickCount { get; set; }
    public uint ClientTickCount { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        ServerTickCount = reader.ReadUInt32();
        ClientTickCount = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}