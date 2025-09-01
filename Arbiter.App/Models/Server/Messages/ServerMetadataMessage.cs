using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.Metadata)]
public class ServerMetadataMessage : IPacketMessage
{
    [InspectSection("Response")]
    [InspectProperty]
    public ServerMetadataResponseType ResponseType { get; set; }

    [InspectSection("Metadata", IsExpandedHandler = nameof(ShouldExpandMetadata))]
    [InspectProperty]
    public string? Name { get; set; }
    
    [InspectProperty(ShowHex = true)]
    public uint? Checksum { get; set; }

    [InspectProperty(ShowMultiline = true)] public IReadOnlyList<byte>? Data { get; set; }

    [InspectSection("Listing", IsExpandedHandler = nameof(ShouldExpandListing))]
    [InspectProperty]
    public List<ServerMetadataEntry>? MetadataFiles { get; set; }

    public void ReadFrom(NetworkPacketReader reader)
    {
        ResponseType = (ServerMetadataResponseType)reader.ReadByte();

        if (ResponseType == ServerMetadataResponseType.Metadata)
        {
            Name = reader.ReadString8();
            Checksum = reader.ReadUInt32();
            
            var contentSize = reader.ReadUInt16();
            Data = reader.ReadBytes(contentSize);
        }
        else if (ResponseType == ServerMetadataResponseType.Listing)
        {
            MetadataFiles = [];

            var count = reader.ReadUInt16();
            for (var i = 0; i < count; i++)
            {
                MetadataFiles.Add(new ServerMetadataEntry
                {
                    Name = reader.ReadString8(),
                    Checksum = reader.ReadUInt32()
                });
            }
        }
    }

    private bool ShouldExpandMetadata() => ResponseType == ServerMetadataResponseType.Metadata;
    private bool ShouldExpandListing() => ResponseType == ServerMetadataResponseType.Listing;
}