namespace Arbiter.Net.Server;

public class ServerPacket(byte command, ReadOnlySpan<byte> payload) : NetworkPacket(command, payload)
{
    public new ServerCommand Command => Enum.IsDefined(typeof(ServerCommand), base.Command)
        ? (ServerCommand)base.Command
        : ServerCommand.Unknown;
}