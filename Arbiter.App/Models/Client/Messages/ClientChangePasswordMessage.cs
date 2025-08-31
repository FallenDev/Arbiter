using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.ChangePassword)]
public class ClientChangePasswordMessage : IPacketMessage
{
    [InspectSection("Credentials")]
    [InspectProperty]
    public string Name { get; set; } = string.Empty;

    [InspectProperty] [InspectMasked] public string CurrentPassword { get; set; } = string.Empty;

    [InspectProperty] [InspectMasked] public string NewPassword { get; set; } = string.Empty;

    public void ReadFrom(NetworkPacketReader reader)
    {
        Name = reader.ReadString8();
        CurrentPassword = reader.ReadString8();
        NewPassword = reader.ReadString8();
    }
}