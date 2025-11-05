using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arbiter.App.Models.Entities;
using Arbiter.App.Models.Player;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientManagerViewModel
{
    private static readonly IReadOnlyList<string> FreeRepairSlotArgs = [Encoding.ASCII.GetString(new byte[] { 1 })];

    private const string TrueLookSkillName = "True Look";
    private const string TrueLookSpellName = "True Look At";
    private const string FreeRepairSkillName = "Mend Item";

    private void AddTrueLookToClients()
    {
        foreach (var client in _clients.Values)
        {
            AddTrueLookSkill(client);
            AddTrueLookTileSpell(client);
        }
    }

    private void RemoveTrueLookFromClients()
    {
        foreach (var client in _clients.Values)
        {
            client.RemoveVirtualSkill(TrueLookSkillName);
            client.RemoveVirtualSpell(TrueLookSpellName);
        }
    }

    private void AddFreeRepairToClients()
    {
        foreach (var client in _clients.Values)
        {
            AddFreeRepairSkill(client);
        }
    }

    private void RemoveFreeRepairFromClients()
    {
        foreach (var client in _clients.Values)
        {
            client.RemoveVirtualSkill(FreeRepairSkillName);
        }
    }

    private static void AddTrueLookSkill(ClientViewModel client)
    {
        if (client.Player.Skillbook.HasSkill(TrueLookSkillName))
        {
            return;
        }

        client.AddVirtualSkill(73, 71, TrueLookSkillName, () => PerformTrueLook(client));
    }

    private static void AddTrueLookTileSpell(ClientViewModel client)
    {
        if (client.Player.Spellbook.HasSpell(TrueLookSpellName))
        {
            return;
        }

        client.AddVirtualSpell(73, 76, TrueLookSpellName, SpellTargetType.PromptTwoNumbers,
            parameters => PerformTrueLookTile(client, parameters), "Enter Map X,Y to Look at: ");
    }

    private void AddFreeRepairSkill(ClientViewModel client)
    {
        if (client.Player.Spellbook.HasSpell(FreeRepairSkillName))
        {
            return;
        }

        client.AddVirtualSkill(73, 62, FreeRepairSkillName, () => PerformFreeRepair(client));
    }

    private static void PerformTrueLook(ClientViewModel client)
    {
        var lookAction = new ClientLookAheadMessage();
        client.EnqueueMessage(lookAction);
    }

    private static void PerformTrueLookTile(ClientViewModel client, SpellCastParameters castParameters)
    {
        if (castParameters.NumericInputs?.Count < 2)
        {
            return;
        }

        var tileX = castParameters.NumericInputs![0];
        var tileY = castParameters.NumericInputs![1];

        var lookTileAction = new ClientLookTileMessage
        {
            TileX = tileX,
            TileY = tileY
        };
        client.EnqueueMessage(lookTileAction);
    }

    private void PerformFreeRepair(ClientViewModel client)
    {
        if (!client.Player.Inventory.TryGetSlot(1, out var item))
        {
            client.SendBarMessage("Nothing to repair.");
            return;
        }

        if (item.MaxDurability < 1)
        {
            client.SendBarMessage($"{item.Name} is not repairable.");
            return;
        }

        if (item.Durability >= item.MaxDurability)
        {
            client.SendBarMessage($"{item.Name} is already fully repaired.");
            return;
        }

        // Find the nearest NPC that can repair the item
        var mapId = client.Player.MapId ?? 0;
        var mapX = client.Player.MapX ?? 0;
        var mapY = client.Player.MapY ?? 0;
        var nearbyNpcs = _entityStore.GetNearbyEntities(mapId, mapX, mapY, 15, EntityFlags.Mundane);

        if (nearbyNpcs.Count == 0)
        {
            client.SendBarMessage("No nearby mundanes to repair.");
            return;
        }

        // Send the repair request
        var entity = nearbyNpcs[0];
        var repairMenuMessage = new ClientDialogMenuChoiceMessage
        {
            EntityType = EntityTypeFlags.Creature,
            EntityId = (uint)entity.Id,
            PursuitId = 0x5B,
            Arguments = FreeRepairSlotArgs.ToList()
        };
        client.EnqueueMessage(repairMenuMessage);
        client.SendBarMessage($"{item.Name} has been repaired by {entity.Name}.");

        // Send cooldown message to prevent spamming
        var slot = client.Player.Skillbook.FindSkill(FreeRepairSkillName);
        if (slot is not null)
        {
            client.EnqueueMessage(new ServerCooldownMessage
            {
                AbilityType = AbilityType.Skill,
                Slot = (byte)slot.Value,
                Seconds = 1
            });
        }
    }
}