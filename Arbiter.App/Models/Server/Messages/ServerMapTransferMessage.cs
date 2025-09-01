using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models.Server.Messages;

[InspectPacket(ServerCommand.MapTransfer)]
public class ServerMapTransferMessage : IPacketMessage
{
    [InspectSection("Tile Data")]
    [InspectProperty]
    public ushort RowY { get; set; }

    [InspectProperty(ShowMultiline = true)]
    public IReadOnlyList<byte> Data { get; set; } = [];

    public void ReadFrom(NetworkPacketReader reader)
    {
        RowY = reader.ReadUInt16();
        Data = reader.ReadToEnd();
    }
}