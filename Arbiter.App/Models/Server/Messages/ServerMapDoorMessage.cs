using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapDoor)]
public class ServerMapDoorMessage : IPacketMessage
{
    [InspectSection("Doors")]
    [InspectProperty]
    public List<ServerMapDoor> Doors { get; set; } = [];

    public void ReadFrom(NetworkPacketReader reader)
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
}