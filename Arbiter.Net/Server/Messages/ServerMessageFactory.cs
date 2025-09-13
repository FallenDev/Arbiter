using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMessageFactory : IServerMessageFactory
{
    public static IServerMessageFactory Default { get; } = new ServerMessageFactory();
    
    private readonly Dictionary<ServerCommand, Type> _typeMappings = new();

    public ServerMessageFactory()
    {
        _typeMappings.Add(ServerCommand.AddEntity, typeof(ServerAddEntityMessage));
        _typeMappings.Add(ServerCommand.AddItem, typeof(ServerAddItemMessage));
        _typeMappings.Add(ServerCommand.AddSkill, typeof(ServerAddSkillMessage));
        _typeMappings.Add(ServerCommand.AddSpell, typeof(ServerAddSpellMessage));
        _typeMappings.Add(ServerCommand.AnimateEntity, typeof(ServerAnimateEntityMessage));
        _typeMappings.Add(ServerCommand.Cooldown, typeof(ServerCooldownMessage));
        _typeMappings.Add(ServerCommand.EntityTurn, typeof(ServerEntityTurnMessage));
        _typeMappings.Add(ServerCommand.EntityWalk, typeof(ServerEntityWalkMessage));
        _typeMappings.Add(ServerCommand.ExitResponse, typeof(ServerExitResponseMessage));
        _typeMappings.Add(ServerCommand.HealthBar, typeof(ServerHealthBarMessage));
        _typeMappings.Add(ServerCommand.Heartbeat, typeof(ServerHeartbeatMessage));
        _typeMappings.Add(ServerCommand.Hello, typeof(ServerHelloMessage));
        _typeMappings.Add(ServerCommand.LightLevel, typeof(ServerLightLevelMessage));
        _typeMappings.Add(ServerCommand.LoginNotice, typeof(ServerLoginNoticeMessage));
        _typeMappings.Add(ServerCommand.LoginResult, typeof(ServerLoginResultMessage));
        _typeMappings.Add(ServerCommand.MapChanged, typeof(ServerMapChangedMessage));
        _typeMappings.Add(ServerCommand.MapChanging, typeof(ServerMapChangingMessage));
        _typeMappings.Add(ServerCommand.MapDoor, typeof(ServerMapDoorMessage));
        _typeMappings.Add(ServerCommand.MapInfo, typeof(ServerMapInfoMessage));
        _typeMappings.Add(ServerCommand.MapLocation, typeof(ServerMapLocationMessage));
        _typeMappings.Add(ServerCommand.MapTransfer, typeof(ServerMapTransferMessage));
        _typeMappings.Add(ServerCommand.MapTransferComplete, typeof(ServerMapTransferCompleteMessage));
        _typeMappings.Add(ServerCommand.Metadata, typeof(ServerMetadataMessage));
        _typeMappings.Add(ServerCommand.PlaySound, typeof(ServerPlaySoundMessage));
        _typeMappings.Add(ServerCommand.PublicMessage, typeof(ServerPublicMessageMessage));
        _typeMappings.Add(ServerCommand.Redirect, typeof(ServerRedirectMessage));
        _typeMappings.Add(ServerCommand.RefreshComplete, typeof(ServerRefreshCompleteMessage));
        _typeMappings.Add(ServerCommand.RemoveEntity, typeof(ServerRemoveEntityMessage));
        _typeMappings.Add(ServerCommand.RemoveItem, typeof(ServerRemoveItemMessage));
        _typeMappings.Add(ServerCommand.RemoveSkill, typeof(ServerRemoveSkillMessage));
        _typeMappings.Add(ServerCommand.RemoveSpell, typeof(ServerRemoveSpellMessage));
        _typeMappings.Add(ServerCommand.RequestUserPortrait, typeof(ServerRequestUserPortraitMessage));
        _typeMappings.Add(ServerCommand.ServerInfo, typeof(ServerServerInfoMessage));
        _typeMappings.Add(ServerCommand.ServerList, typeof(ServerServerListMessage));
        _typeMappings.Add(ServerCommand.ServerTable, typeof(ServerServerTableMessage));
        _typeMappings.Add(ServerCommand.SetEquipment, typeof(ServerSetEquipmentMessage));
        _typeMappings.Add(ServerCommand.ShowEffect, typeof(ServerShowEffectMessage));
        _typeMappings.Add(ServerCommand.ShowNotepad, typeof(ServerShowNotepadMessage));
        _typeMappings.Add(ServerCommand.ShowPlayer, typeof(ServerShowPlayerMessage));
        _typeMappings.Add(ServerCommand.UpdateStats, typeof(ServerUpdateStatsMessage));
        _typeMappings.Add(ServerCommand.UserId, typeof(ServerUserIdMessage));
        _typeMappings.Add(ServerCommand.WalkResponse, typeof(ServerWalkResponseMessage));
        _typeMappings.Add(ServerCommand.WorldMessage, typeof(ServerWorldMessageMessage));
    }
    
    public Type? GetMessageType(ServerCommand command) => _typeMappings.GetValueOrDefault(command);

    public ServerCommand? GetMessageCommand(Type messageType)
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