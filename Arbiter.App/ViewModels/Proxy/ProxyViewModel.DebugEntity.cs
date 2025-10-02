using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugAddEntityFilterName = "Debug_AddEntityFilter";

    private void AddDebugEntityFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.AddEntity, new NetworkPacketFilter(HandleAddEntityMessage, settings)
        {
            Name = DebugAddEntityFilterName,
            Priority = int.MaxValue
        });
    }

    private void RemoveDebugEntityFilters() =>
        _proxyServer.RemoveFilter(ServerCommand.AddEntity, DebugAddEntityFilterName);
    
    private NetworkPacket HandleAddEntityMessage(NetworkPacket packet, object? parameter)
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
}