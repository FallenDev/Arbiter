using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMessageFactory : IServerMessageFactory
{
    public Type? GetMessageType(ServerCommand command) => command switch
    {
        ServerCommand.AddEntity => typeof(ServerAddEntityMessage),
        ServerCommand.AddItem => typeof(ServerAddItemMessage),
        ServerCommand.AddSkill => typeof(ServerAddSkillMessage),
        ServerCommand.AddSpell => typeof(ServerAddSpellMessage),
        ServerCommand.AnimateEntity => typeof(ServerAnimateEntityMessage),
        ServerCommand.Cooldown => typeof(ServerCooldownMessage),
        ServerCommand.EntityTurn => typeof(ServerEntityTurnMessage),
        ServerCommand.EntityWalk => typeof(ServerEntityWalkMessage),
        ServerCommand.ExitResponse => typeof(ServerExitResponseMessage),
        ServerCommand.Heartbeat => typeof(ServerHeartbeatMessage),
        ServerCommand.Hello => typeof(ServerHelloMessage),
        ServerCommand.LightLevel => typeof(ServerLightLevelMessage),
        ServerCommand.LoginNotice => typeof(ServerLoginNoticeMessage),
        ServerCommand.LoginResult => typeof(ServerLoginResultMessage),
        ServerCommand.MapChanged => typeof(ServerMapChangedMessage),
        ServerCommand.MapChanging => typeof(ServerMapChangingMessage),
        ServerCommand.MapDoor => typeof(ServerMapDoorMessage),
        ServerCommand.MapInfo => typeof(ServerMapInfoMessage),
        ServerCommand.MapTransfer => typeof(ServerMapTransferMessage),
        ServerCommand.MapTransferComplete => typeof(ServerMapTransferCompleteMessage),
        ServerCommand.Metadata => typeof(ServerMetadataMessage),
        ServerCommand.PlaySound => typeof(ServerPlaySoundMessage),
        ServerCommand.PublicMessage => typeof(ServerPublicMessageMessage),
        ServerCommand.Redirect => typeof(ServerRedirectMessage),
        ServerCommand.RemoveEntity => typeof(ServerRemoveEntityMessage),
        ServerCommand.RemoveItem => typeof(ServerRemoveItemMessage),
        ServerCommand.RemoveSkill => typeof(ServerRemoveSkillMessage),
        ServerCommand.RemoveSpell => typeof(ServerRemoveSpellMessage),
        ServerCommand.RequestUserPortrait => typeof(ServerRequestUserPortraitMessage),
        ServerCommand.ServerInfo => typeof(ServerServerInfoMessage),
        ServerCommand.ServerList => typeof(ServerServerListMessage),
        ServerCommand.ServerTable => typeof(ServerServerTableMessage),
        ServerCommand.SetEquipment => typeof(ServerSetEquipmentMessage),
        ServerCommand.ShowPlayer => typeof(ServerShowPlayerMessage),
        ServerCommand.UpdateStats => typeof(ServerUpdateStatsMessage),
        ServerCommand.UserId => typeof(ServerUserIdMessage),
        ServerCommand.WalkResponse => typeof(ServerWalkResponseMessage),
        ServerCommand.WorldMessage => typeof(ServerWorldMessageMessage),
        _ => null
    };

    public IServerMessage? Create(ServerPacket packet)
    {
        var type = GetMessageType(packet.Command);
        if (type is null)
        {
            return null;
        }

        var instance = (IServerMessage)Activator.CreateInstance(type)!;
        var reader = new NetworkPacketReader(packet);
        instance.Deserialize(reader);

        return instance;
    }
}