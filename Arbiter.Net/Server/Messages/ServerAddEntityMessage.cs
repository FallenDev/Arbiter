using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.AddEntity)]
public class ServerAddEntityMessage : ServerMessage
{
    public List<ServerEntityObject> Entities { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        var entityCount = reader.ReadUInt16();

        for (var i = 0; i < entityCount; i++)
        {
            var x = reader.ReadUInt16();
            var y = reader.ReadUInt16();
            var id = reader.ReadUInt32();
            var sprite = reader.ReadUInt16();

            var isCreature = (sprite & SpriteFlags.Creature) > 0;

            if (isCreature)
            {
                var unknown = reader.ReadUInt32();
                var direction = (WorldDirection)reader.ReadByte();

                reader.Skip(1); // not sure what this is for

                var creatureType = (CreatureType)reader.ReadByte();
                var creatureName = creatureType == CreatureType.Mundane ? reader.ReadString8() : null;

                Entities.Add(new ServerCreatureEntity
                {
                    Id = id,
                    X = x,
                    Y = y,
                    Sprite = sprite,
                    Direction = direction,
                    CreatureType = creatureType,
                    Name = creatureName,
                    Unknown = unknown,
                });
            }
            else
            {
                var color = (DyeColor)reader.ReadByte();
                var unknown = reader.ReadUInt16();

                Entities.Add(new ServerItemEntity
                {
                    Id = id,
                    X = x,
                    Y = y,
                    Sprite = sprite,
                    Color = color,
                    Unknown = unknown,
                });
            }
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}