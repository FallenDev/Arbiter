using Arbiter.Net.Annotations;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.LookAhead)]
public class ClientLookAheadMessage : ClientMessage
{
    // No additional data
}