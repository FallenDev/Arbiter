namespace Arbiter.Net.Server;

public class ServerPacket : NetworkPacket
{
    public byte? Sequence { get; set; }
    
    public new ServerCommand Command => Enum.IsDefined(typeof(ServerCommand), base.Command)
        ? (ServerCommand)base.Command
        : ServerCommand.Unknown;

    public ServerPacket(byte command, ReadOnlySpan<byte> payload)
        : base(command, payload)
    {

    }

    public ServerPacket(byte command, IEnumerable<byte> payload)
        : base(command, payload.ToArray())
    {

    }

    public override IEnumerator<byte> GetEnumerator()
    {
        var size = Data.Length + (Sequence.HasValue ? 2 : 1);

        yield return Marker;
        yield return (byte)((size >> 8) & 0xFF);
        yield return (byte)(size & 0xFF);
        yield return (byte)Command;

        if (Sequence is not null)
        {
            yield return Sequence.Value;
        }

        foreach (var dataByte in Data)
        {
            yield return dataByte;
        }
    }
}