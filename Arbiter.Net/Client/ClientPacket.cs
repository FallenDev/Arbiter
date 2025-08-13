namespace Arbiter.Net.Client;

public class ClientPacket : NetworkPacket
{
    public byte? Sequence { get; set; }
    public uint? Checksum { get; }

    public new ClientCommand Command => Enum.IsDefined(typeof(ClientCommand), base.Command)
        ? (ClientCommand)base.Command
        : ClientCommand.Unknown;

    public ClientPacket(byte command, ReadOnlySpan<byte> payload, uint? checksum = null) : base(command, payload)
    {
        Checksum = checksum;
    }

    public ClientPacket(byte command, IEnumerable<byte> payload, uint? checksum = null)
        : base(command, payload.ToArray())
    {
        Checksum = checksum;
    }
}