namespace Arbiter.Net.Server;

public enum ServerCommand : byte
{
    SetEncryption = 0x00,
    ServerMessage = 0x02,
    Redirect = 0x03,
    UserId = 0x05,
    ServerTable = 0x56,
    LoginNotice = 0x60,
    LoginInfo = 0x66,
    Hello = 0x7E,
    Unknown = 0xFF,
}