namespace Arbiter.Net.Client;

public enum ClientCommand : byte
{
    ClientVersion = 0x00,
    Authenticate = 0x10,
    ServerTable = 0x57,
    Unknown = 0xFF
}