namespace Arbiter.Net.Server;

public enum ServerCommand : byte
{
    Redirect = 0x03,
    SetUserId = 0x05,
    Unknown = 0xFF,
}