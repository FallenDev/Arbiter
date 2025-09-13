using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMetadataMessage : ServerMessage
{
    public ServerMetadataResponseType ResponseType { get; set; }
    public string? Name { get; set; }
    public uint? Checksum { get; set; }
    public IReadOnlyList<byte>? Data { get; set; }
    public List<ServerMetadataEntry>? MetadataFiles { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
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
    
    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}