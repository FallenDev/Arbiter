using System.Net;
using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.Redirect)]
public class ServerRedirectMessage : ServerMessage
{
    public IPAddress Address { get; set; } = IPAddress.None;
    public ushort Port { get; set; }
    public byte Seed { get; set; }
    public IReadOnlyList<byte> PrivateKey { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public uint ConnectionId { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Address = reader.ReadIPv4Address();
        Port = reader.ReadUInt16();

        reader.Skip(1); // remaining count

        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);

        Name = reader.ReadString8();
        ConnectionId = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}