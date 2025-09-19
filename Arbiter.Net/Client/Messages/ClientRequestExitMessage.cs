using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.RequestExit)]
public class ClientRequestExitMessage : ClientMessage
{
    public ClientExitReason Reason { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        if (reader.CanRead(1))
        {
            Reason = (ClientExitReason)reader.ReadByte();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}