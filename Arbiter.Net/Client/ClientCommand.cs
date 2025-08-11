namespace Arbiter.Net.Client;

public enum ClientCommand : byte
{
    ClientVersion = 0x00,
    Login = 0x03,
    Exit = 0x0B,
    Authenticate = 0x10,
    RequestLoginNotice = 0x4B,
    RequestServerTable = 0x57,
    RequestSequence = 0x62,
    RequestHomepage = 0x68,
    Unknown = 0xFF
}