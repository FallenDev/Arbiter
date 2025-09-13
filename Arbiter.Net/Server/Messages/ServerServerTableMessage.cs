using Arbiter.Net.Annotations;
using Arbiter.Net.Compression;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ServerTable)]
public class ServerServerTableMessage : ServerMessage
{
    public List<ServerTableEntry> Servers { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // The server table is compressed with zlib
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

            Servers.Add(new ServerTableEntry
            {
                Id = serverId,
                Address = serverAddress,
                Port = serverPort,
                Name = serverName,
            });
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}