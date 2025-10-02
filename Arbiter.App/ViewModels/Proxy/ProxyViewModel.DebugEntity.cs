using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugAddEntityFilterName = "Debug_AddEntityFilter";
    private const string DebugInteractFilterName = "Debug_InteractFilter";
    private const string DebugInteractMessageFilterName = "Debug_InteractMessageFilter";
    
    private void AddDebugEntityFilters(DebugSettings settings)
    {
        if (settings.ShowNpcId || settings.ShowMonsterId)
        {
            _proxyServer.AddFilter(ServerCommand.AddEntity, new NetworkPacketFilter(HandleAddEntityMessage, settings)
            {
                Name = DebugAddEntityFilterName,
                Priority = int.MaxValue
            });
        }

        if (settings.ShowMonsterClickId)
        {
            _proxyServer.AddFilter(ClientCommand.Interact, new NetworkPacketFilter(HandleInteractMessage, settings)
            {
                Name = DebugInteractFilterName,
                Priority = int.MaxValue
            });
            
            _proxyServer.AddFilter(ServerCommand.WorldMessage, new NetworkPacketFilter(HandleInteractResponseMessage, settings)
            {
                Name = DebugInteractMessageFilterName,
                Priority = int.MaxValue
            });
        }
    }

    private void RemoveDebugEntityFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.AddEntity, DebugAddEntityFilterName);
        _proxyServer.RemoveFilter(ClientCommand.Interact, DebugInteractFilterName);
        _proxyServer.RemoveFilter(ServerCommand.WorldMessage, DebugInteractMessageFilterName);
    }

    private NetworkPacket HandleAddEntityMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterId: false, ShowNpcId: false } ||
            !_serverMessageFactory.TryCreate<ServerAddEntityMessage>(serverPacket, out var message))
        {
            return packet;
        }

        // Inject NPC IDs into the entity names
        if (filterSettings.ShowNpcId)
        {
            foreach (var entity in message.Entities)
            {
                if (entity is not ServerCreatureEntity { CreatureType: CreatureType.Mundane } npcEntity)
                {
                    continue;
                }

                var name = npcEntity.Name ?? npcEntity.CreatureType.ToString();
                npcEntity.Name = $"{name} 0x{npcEntity.Id:X4}";
            }
        }

        // Inject monster IDs into the entity names
        if (filterSettings.ShowMonsterId)
        {
            foreach (var entity in message.Entities)
            {
                if (entity is not ServerCreatureEntity { CreatureType: CreatureType.Monster } monsterEntity)
                {
                    continue;
                }

                var name = monsterEntity.Name ?? monsterEntity.CreatureType.ToString();
                
                // Need to set the creature type to Mundane to display hover name
                monsterEntity.CreatureType = CreatureType.Mundane;
                monsterEntity.Name = $"{name} 0x{monsterEntity.Id:X4}";
            }
        }

        // Build a new packet with the modified entity data
        var builder = new NetworkPacketBuilder(ServerCommand.AddEntity);
        message.Serialize(builder);

        return builder.ToPacket();
    }

    private NetworkPacket HandleInteractMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ClientPacket clientPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterClickId: false } ||
            !_clientMessageFactory.TryCreate<ClientInteractMessage>(clientPacket, out var message))
        {
            return packet;
        }

        if (message is { InteractionType: InteractionType.Entity, TargetId: not null })
        {
            
        }

        return packet;
    }
    
    private NetworkPacket HandleInteractResponseMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowMonsterClickId: false } ||
            !_serverMessageFactory.TryCreate<ServerWorldMessageMessage>(serverPacket, out var message))
        {
            return packet;
        }

        return packet;
    }
}