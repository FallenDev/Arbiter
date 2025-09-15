using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.WorldList)]
public class ServerWorldListMessage : ServerMessage
{
    public ushort WorldCount { get; set; }
    public ushort CountryCount { get; set; }
    public List<ServerWorldListUser> Users { get; set; } = [];
    
    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
       WorldCount = reader.ReadUInt16();
       CountryCount = reader.ReadUInt16();
       
        for (var i = 0; i < CountryCount; i++)
        {
            var classWithFlags = reader.ReadByte();
            
            Users.Add(new ServerWorldListUser
            {
                Class = (CharacterClass)(classWithFlags & 0x07),
                Flags = (byte)(classWithFlags & 0xF8),
                Color = (WorldListColor)reader.ReadByte(),
                Status = (SocialStatus)reader.ReadByte(),
                Title = reader.ReadString8(),
                IsMaster = reader.ReadBoolean(),
                Name = reader.ReadString8()
            });
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}