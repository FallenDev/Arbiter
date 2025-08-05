namespace Arbiter.Net.Server;

public class ServerPacket : NetworkPacket
{
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
}