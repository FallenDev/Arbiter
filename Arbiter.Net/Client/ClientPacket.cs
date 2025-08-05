namespace Arbiter.Net.Client;

public class ClientPacket(byte command, ReadOnlySpan<byte> payload, uint? checksum = null) : NetworkPacket(command, payload)
{
    public uint? Checksum { get; } = checksum;
    
    public new ClientCommand Command => Enum.IsDefined(typeof(ClientCommand), base.Command)
        ? (ClientCommand)base.Command
        : ClientCommand.Unknown;
}