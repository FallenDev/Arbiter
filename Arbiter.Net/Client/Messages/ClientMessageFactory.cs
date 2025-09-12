using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientMessageFactory : IClientMessageFactory
{
    public Type? GetMessageType(ClientCommand command) => command switch
    {
        ClientCommand.Assail => typeof(ClientAssailMessage),
        ClientCommand.Authenticate => typeof(ClientAuthenticateMessage),
        ClientCommand.ChangePassword => typeof(ClientChangePasswordMessage),
        ClientCommand.CreateCharacterName => typeof(ClientCreateCharacterNameMessage),
        ClientCommand.DropItem => typeof(ClientDropItemMessage),
        ClientCommand.Heartbeat => typeof(ClientHeartbeatMessage),
        ClientCommand.Login => typeof(ClientLoginMessage),
        ClientCommand.PickupItem => typeof(ClientPickupItemMessage),
        ClientCommand.RequestExit => typeof(ClientRequestExitMessage),
        ClientCommand.RequestHomepage => typeof(ClientRequestHomepageMessage),
        ClientCommand.RequestLoginNotice => typeof(ClientRequestLoginNoticeMessage),
        ClientCommand.RequestMetadata => typeof(ClientRequestMetadataMessage),
        ClientCommand.RequestProfile => typeof(ClientRequestProfileMessage),
        ClientCommand.RequestSequence => typeof(ClientRequestSequenceMessage),
        ClientCommand.RequestServerTable => typeof(ClientRequestServerTableMessage),
        ClientCommand.Turn => typeof(ClientTurnMessage),
        ClientCommand.UserPortrait => typeof(ClientUserPortraitMessage),
        ClientCommand.Version => typeof(ClientVersionMessage),
        ClientCommand.Walk => typeof(ClientWalkMessage),
        _ => null
    };

    public IClientMessage? Create(ClientPacket packet)
    {
        var type = GetMessageType(packet.Command);
        if (type is null)
        {
            return null;
        }

        var instance = (IClientMessage)Activator.CreateInstance(type)!;
        var reader = new NetworkPacketReader(packet);
        instance.Deserialize(reader);

        return instance;
    }
}