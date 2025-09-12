using System.Text;
using Arbiter.Net.Compression;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerLoginNoticeMessage : ServerMessage
{
    public uint? Checksum { get; set; }
    public string? Content { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        var hasContent = reader.ReadBoolean();

        if (!hasContent)
        {
            Checksum = reader.ReadUInt32();
            return;
        }

        // The server notice is compressed with zlib
        var contentLength = reader.ReadUInt16();
        var compressed = reader.ReadBytes(contentLength);

        var decompressed = Zlib.Decompress(compressed);
        Content = Encoding.UTF8.GetString(decompressed);
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}