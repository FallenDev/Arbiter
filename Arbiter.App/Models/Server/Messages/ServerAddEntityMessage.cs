using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.AddEntity)]
public class ServerAddEntityMessage : IPacketMessage
{
    [InspectSection("Entities")]
    [InspectProperty]
    public List<ServerEntityObject> Entities { get; set; } = [];

    public void ReadFrom(NetworkPacketReader reader)
    {
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
                var color = (ItemColor)reader.ReadByte();
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
}