using System.Collections.Generic;
using System.Net;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Packets.Server;

[InspectPacket(ServerCommand.Redirect)]
public class ServerRedirectMessage : IPacketMessage
{
    [InspectSection("Destination")]
    [InspectProperty]
    public IPAddress Address { get; set; } = IPAddress.None;

    [InspectProperty] public ushort Port { get; set; }

    [InspectSection("Encryption")]
    [InspectProperty]
    public byte Seed { get; set; }

    [InspectProperty] public IReadOnlyList<byte> PrivateKey { get; set; } = [];

    [InspectSection("Connection")]
    [InspectProperty(Name = "Id")]
    public uint ConnectionId { get; set; }

    [InspectProperty] public string Name { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        Address = reader.ReadIPv4Address();
        Port = reader.ReadUInt16();

        reader.Skip(1); // remaining count

        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);

        Name = reader.ReadString8();
        ConnectionId = reader.ReadUInt32();
    }
}