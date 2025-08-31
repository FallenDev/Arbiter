using System.Text;
using Arbiter.App.Annotations;
using Arbiter.Common.IO.Compression;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.LoginNotice)]
public class ServerLoginNoticeMessage : IPacketMessage
{
    [InspectSection("Login Notice")]
    [InspectProperty(ShowHex = true)]
    public uint? Checksum { get; set; }
    
    [InspectProperty(ShowMultiline = true)]
    public string? Notice { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
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
        Notice = Encoding.UTF8.GetString(decompressed);
    }
}