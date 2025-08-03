namespace Arbiter.Net.Client;

public class ClientPacket(byte command, ReadOnlySpan<byte> payload) : NetworkPacket(command, payload)
{
    public new ClientCommand Command => Enum.IsDefined(typeof(ClientCommand), base.Command)
        ? (ClientCommand)base.Command
        : ClientCommand.Unknown;
}