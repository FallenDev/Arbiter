using Arbiter.Net.Serialization;
using Arbiter.Net.Types;

namespace Arbiter.Net.Server.Messages;

public class ServerMapDoorMessage : INetworkSerializable
{
    public List<ServerMapDoor> Doors { get; set; } = [];

    public void Deserialize(INetworkPacketReader reader)
    {
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

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}