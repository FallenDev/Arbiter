namespace Arbiter.Net.Server;

public enum ServerCommand : byte
{
    Unknown = 0x00,
    Redirect = 0x03,
    SetPlayerId = 0x05
}