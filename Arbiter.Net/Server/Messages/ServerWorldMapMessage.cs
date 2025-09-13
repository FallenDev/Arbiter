using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.WorldMap)]
public class ServerWorldMapMessage : ServerMessage
{
    public string FieldName { get; set; } = string.Empty;
    public byte FieldIndex { get; set; }
    
    public List<ServerWorldMapNode> Locations { get; set; } = [];
    
    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        FieldName = reader.ReadString8();

        var nodeCount = reader.ReadByte();
        
        FieldIndex = reader.ReadByte();

        for (var i = 0; i < nodeCount; i++)
        {
            var node = new ServerWorldMapNode
            {
                ScreenX = reader.ReadUInt16(),
                ScreenY = reader.ReadUInt16(),
                Name = reader.ReadString8(),
                Checksum = reader.ReadUInt16(),
                MapId = reader.ReadUInt16(),
                MapX = reader.ReadUInt16(),
                MapY = reader.ReadUInt16()
            };
            
            Locations.Add(node);
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}