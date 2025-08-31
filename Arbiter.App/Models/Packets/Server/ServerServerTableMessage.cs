using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Common.IO;
using Arbiter.Common.IO.Compression;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Packets.Server;

[InspectPacket(ServerCommand.ServerTable)]
public class ServerServerTableMessage : IPacketMessage
{
    [InspectSection("Server Table")]
    [InspectProperty] public List<ServerTableEntry> Servers { get; set; } = [];

    public void ReadFrom(NetworkPacketReader reader)
    {
        var contentLength = reader.ReadUInt16();
        var compressed = reader.ReadBytes(contentLength);

        var decompressed = Zlib.Decompress(compressed);

        var tableReader = new SpanReader(Endianness.BigEndian);
        var serverCount = tableReader.ReadByte(decompressed);

        for (var i = 0; i < serverCount; i++)
        {
            var serverId = tableReader.ReadByte(decompressed);
            var serverAddress = tableReader.ReadIPv4Address(decompressed);
            var serverPort = tableReader.ReadUInt16(decompressed);
            var serverName = tableReader.ReadNullTerminatedString(decompressed);

            Servers.Add(new ServerTableEntry(serverId, serverName, serverAddress, serverPort));
        }
    }
}