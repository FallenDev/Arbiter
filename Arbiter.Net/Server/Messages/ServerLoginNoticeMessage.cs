using System.Text;
using Arbiter.Net.Annotations;
using Arbiter.Net.Compression;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.LoginNotice)]
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
        base.Serialize(builder);

        if (string.IsNullOrEmpty(Content))
        {
            builder.AppendBoolean(false);
            builder.AppendUInt32(Checksum ?? 0);
            return;
        }

        builder.AppendBoolean(true);
        var bytes = Encoding.UTF8.GetBytes(Content);
        var compressed = Zlib.Compress(bytes);
        builder.AppendUInt16((ushort)compressed.Length);
        builder.AppendBytes(compressed);
    }
}