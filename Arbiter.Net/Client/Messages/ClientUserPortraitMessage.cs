using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientUserPortraitMessage : ClientMessage
{
    public IReadOnlyList<byte> Portrait { get; set; } = [];

    public string Bio { get; set; } = string.Empty;

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        var totalLength = reader.ReadUInt16();
        if (totalLength <= 0)
        {
            return;
        }

        var portraitLength = reader.ReadUInt16();
        Portrait = reader.ReadBytes(portraitLength);
        Bio = reader.ReadString16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }

    // Migrate these
    private bool ShouldShowPortrait() => Portrait.Count > 0;
    private bool ShouldShowBio() => !string.IsNullOrEmpty(Bio);
}