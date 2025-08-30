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

    public override IEnumerator<byte> GetEnumerator()
    {
        var size = Data.Length + (Sequence.HasValue ? 2 : 1);
        if (Checksum.HasValue)
        {
            size += 4;
        }

        yield return Marker;
        yield return (byte)((size >> 8) & 0xFF);
        yield return (byte)(size & 0xFF);
        yield return (byte)Command;

        if (Sequence.HasValue)
        {
            yield return Sequence.Value;
        }

        foreach (var dataByte in Data)
        {
            yield return dataByte;
        }

        if (!Checksum.HasValue)
        {
            yield break;
        }

        yield return (byte)((Checksum.Value >> 24) & 0xFF);
        yield return (byte)((Checksum.Value >> 16) & 0xFF);
        yield return (byte)((Checksum.Value >> 8) & 0xFF);
        yield return (byte)(Checksum.Value & 0xFF);
    }
}