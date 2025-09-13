using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientMessageFactory : IClientMessageFactory
{
    public static IClientMessageFactory Default { get; } = new ClientMessageFactory();

    private readonly Dictionary<ClientCommand, Type> _typeMappings = new();

    public ClientMessageFactory()
    {
        _typeMappings.Add(ClientCommand.Assail, typeof(ClientAssailMessage));
        _typeMappings.Add(ClientCommand.Authenticate, typeof(ClientAuthenticateMessage));
        _typeMappings.Add(ClientCommand.ChangePassword, typeof(ClientChangePasswordMessage));
        _typeMappings.Add(ClientCommand.CreateCharacterName, typeof(ClientCreateCharacterNameMessage));
        _typeMappings.Add(ClientCommand.DropItem, typeof(ClientDropItemMessage));
        _typeMappings.Add(ClientCommand.Heartbeat, typeof(ClientHeartbeatMessage));
        _typeMappings.Add(ClientCommand.Interact, typeof(ClientInteractMessage));
        _typeMappings.Add(ClientCommand.Login, typeof(ClientLoginMessage));
        _typeMappings.Add(ClientCommand.PickupItem, typeof(ClientPickupItemMessage));
        _typeMappings.Add(ClientCommand.RequestExit, typeof(ClientRequestExitMessage));
        _typeMappings.Add(ClientCommand.RequestHomepage, typeof(ClientRequestHomepageMessage));
        _typeMappings.Add(ClientCommand.RequestLoginNotice, typeof(ClientRequestLoginNoticeMessage));
        _typeMappings.Add(ClientCommand.RequestMetadata, typeof(ClientRequestMetadataMessage));
        _typeMappings.Add(ClientCommand.RequestProfile, typeof(ClientRequestProfileMessage));
        _typeMappings.Add(ClientCommand.RequestSequence, typeof(ClientRequestSequenceMessage));
        _typeMappings.Add(ClientCommand.RequestServerTable, typeof(ClientRequestServerTableMessage));
        _typeMappings.Add(ClientCommand.SwapSlot, typeof(ClientSwapSlotMessage));
        _typeMappings.Add(ClientCommand.Turn, typeof(ClientTurnMessage));
        _typeMappings.Add(ClientCommand.UserPortrait, typeof(ClientUserPortraitMessage));
        _typeMappings.Add(ClientCommand.Version, typeof(ClientVersionMessage));
        _typeMappings.Add(ClientCommand.Walk, typeof(ClientWalkMessage));
        _typeMappings.Add(ClientCommand.WorldMapClick, typeof(ClientWorldMapClickMessage));
    }

    public Type? GetMessageType(ClientCommand command) => _typeMappings.GetValueOrDefault(command);

    public ClientCommand? GetMessageCommand(Type messageType)
    {
        foreach (var (key, value) in _typeMappings)
        {
            if (value == messageType)
            {
                return key;
            }
        }

        return null;
    }

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