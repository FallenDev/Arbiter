using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.Authenticate)]
public class ClientAuthenticateMessage : IPacketMessage
{
    [InspectSection("Encryption")]
    [InspectProperty]
    public byte Seed { get; set; }

    [InspectProperty(ShowMultiline = true)]
    public IReadOnlyList<byte> PrivateKey { get; set; } = [];

    [InspectSection("Connection")]
    [InspectProperty(Name = "Id")]
    public uint ConnectionId { get; set; }

    [InspectProperty] public string Name { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);
        Name = reader.ReadString8();
        ConnectionId = reader.ReadUInt32();
    }
}