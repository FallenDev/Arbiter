using Arbiter.App.Annotations;
using Arbiter.Net;
using Arbiter.Net.Client;

namespace Arbiter.App.Models.Client.Messages;

[InspectPacket(ClientCommand.CreateCharacterName)]
public class ClientCreateCharacterNameMessage : IPacketMessage
{
    [InspectSection("Credentials")]
    [InspectProperty]
    public string Name { get; set; } = string.Empty;
    
    [InspectProperty]
    [InspectMasked]
    public string Password { get; set; } = string.Empty;

    [InspectProperty] public string Email { get; set; } = string.Empty;
    
    public void ReadFrom(NetworkPacketReader reader)
    {
        Name = reader.ReadString8();
        Password = reader.ReadString8();
        Email = reader.ReadString8();
    }
}