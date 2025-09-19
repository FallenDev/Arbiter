using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.Metadata)]
public class ServerMetadataMessage : ServerMessage
{
    public MetadataResponseType ResponseType { get; set; }
    public string? Name { get; set; }
    public uint? Checksum { get; set; }
    public IReadOnlyList<byte>? Data { get; set; }
    public List<ServerMetadataEntry>? MetadataFiles { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        ResponseType = (MetadataResponseType)reader.ReadByte();

        if (ResponseType == MetadataResponseType.Metadata)
        {
            Name = reader.ReadString8();
            Checksum = reader.ReadUInt32();
            
            var contentSize = reader.ReadUInt16();
            Data = reader.ReadBytes(contentSize);
        }
        else if (ResponseType == MetadataResponseType.Listing)
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