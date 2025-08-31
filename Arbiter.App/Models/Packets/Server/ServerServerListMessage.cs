using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Packets.Server;

[InspectPacket(ServerCommand.ServerList)]
public class ServerServerListMessage : IPacketMessage
{
    [InspectSection("Server Table")]
    [InspectProperty(ShowHex = true)]
    public uint Checksum { get; set; }
    
    [InspectSection("Encryption")]
    [InspectProperty]
    public byte Seed { get; set; }

    [InspectProperty] public IReadOnlyList<byte> PrivateKey { get; set; } = [];

    public void ReadFrom(NetworkPacketReader reader)
    {
        // Not sure what this byte is for
        reader.Skip(1);

        Checksum = reader.ReadUInt32();
        Seed = reader.ReadByte();

        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);
    }
}