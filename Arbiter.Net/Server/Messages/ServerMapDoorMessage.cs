using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.MapDoor)]
public class ServerMapDoorMessage : ServerMessage
{
    public List<ServerMapDoor> Doors { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        var count = reader.ReadByte();

        for (var i = 0; i < count; i++)
        {
            var door = new ServerMapDoor
            {
                X = reader.ReadByte(),
                Y = reader.ReadByte(),
                State = (DoorState)reader.ReadByte(),
                Direction = (DoorDirection)reader.ReadByte()
            };

            Doors.Add(door);
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}