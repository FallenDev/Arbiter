using System.Collections.Generic;
using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.UserPortrait)]
public class ClientUserPortraitMessage : IPacketMessage
{
    [InspectSection("Portrait", IsExpandedHandler = nameof(ShouldShowPortrait))]
    [InspectProperty(ShowMultiline = true)]
    public IReadOnlyList<byte> Portrait { get; set; } = [];
    
    [InspectSection("Bio", IsExpandedHandler = nameof(ShouldShowBio))]
    [InspectProperty(ShowMultiline = true)]
    public string Bio { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        var totalLength = reader.ReadUInt16();
        if (totalLength <= 0)
        {
            return;
        }

        var portraitLength = reader.ReadUInt16();
        Portrait = reader.ReadBytes(portraitLength);
        Bio = reader.ReadString16();
    }
    
    private bool ShouldShowPortrait() => Portrait.Count > 0;
    private bool ShouldShowBio() => !string.IsNullOrEmpty(Bio);
}