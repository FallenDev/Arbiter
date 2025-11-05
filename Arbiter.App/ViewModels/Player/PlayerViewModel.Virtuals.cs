using System.Collections.Concurrent;
using System.Threading.Tasks;
using Arbiter.App.Models.Player;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Client.Types;
using Arbiter.Net.Filters;
using Arbiter.Net.Observers;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerViewModel
{
    private const string FilterPrefix = nameof(PlayerViewModel);

    private readonly ConcurrentDictionary<int, (InventoryItem Item, int TargetSlot)> _pendingVirtualItemSwaps = new();
    private readonly ConcurrentDictionary<int, (SkillbookItem Skill, int TargetSlot)> _pendingVirtualSkillSwaps = new();
    private readonly ConcurrentDictionary<int, (SpellbookItem Spell, int TargetSlot)> _pendingVirtualSpellSwaps = new();

    private NetworkFilterRef? _virtualItemFilter;
    private NetworkFilterRef? _virtualSkillFilter;
    private NetworkFilterRef? _virtualSpellFilter;
    private NetworkFilterRef? _virtualSlotFilter;

    private NetworkObserverRef? _virtualItemRemoveObserver;
    private NetworkObserverRef? _virtualSkillRemoveObserver;
    private NetworkObserverRef? _virtualSpellRemoveObserver;

    private void AddVirtualFilters(ProxyConnection connection)
    {
        _virtualItemFilter = connection.AddFilter<ClientUseItemMessage>(HandleClientUseItemMessage,
            $"{FilterPrefix}_VirtualItemFilter",
            int.MaxValue);

        _virtualSkillFilter = connection.AddFilter<ClientUseSkillMessage>(HandleClientUseSkillMessage,
            $"{FilterPrefix}_VirtualSkillFilter", int.MaxValue);

        _virtualSpellFilter = connection.AddFilter<ClientCastSpellMessage>(HandleClientCastSpellMessage,
            $"{FilterPrefix}_VirtualSpellFilter", int.MaxValue);

        _virtualSlotFilter = connection.AddFilter<ClientSwapSlotMessage>(HandleClientSwapSlotMessage,
            $"{FilterPrefix}_VirtualSlotFilter", int.MaxValue);

        _virtualItemRemoveObserver =
            connection.AddObserver<ServerRemoveItemMessage>(HandleServerRemoveItemMessage, int.MaxValue);
        _virtualSkillRemoveObserver =
            connection.AddObserver<ServerRemoveSkillMessage>(HandleServerRemoveSkillMessage, int.MaxValue);
        _virtualSpellRemoveObserver =
            connection.AddObserver<ServerRemoveSpellMessage>(HandleServerRemoveSpellMessage, int.MaxValue);
    }

    private void RemoveVirtualFilters()
    {
        _virtualItemFilter?.Unregister();
        _virtualSkillFilter?.Unregister();
        _virtualSpellFilter?.Unregister();
        _virtualSlotFilter?.Unregister();

        _virtualItemRemoveObserver?.Unregister();
        _virtualSkillRemoveObserver?.Unregister();
        _virtualSpellRemoveObserver?.Unregister();
    }

    private NetworkPacket? HandleClientUseItemMessage(ProxyConnection connection, ClientUseItemMessage message,
        object? parameter, NetworkMessageFilterResult<ClientUseItemMessage> result)
    {
        if (!Inventory.TryGetSlot(message.Slot, out var slotted) || !slotted.Value.IsVirtual is not true)
        {
            return result.Passthrough();
        }

        // Block virtual using of items but still fire off the action internally
        var item = slotted.Value;
        if (item.OnUse is not null)
        {
            Task.Run(() => item.OnUse());
        }

        return result.Block();
    }

    private NetworkPacket? HandleClientUseSkillMessage(ProxyConnection connection, ClientUseSkillMessage message,
        object? parameter, NetworkMessageFilterResult<ClientUseSkillMessage> result)
    {
        if (!Skills.TryGetSlot(message.Slot, out var slotted) || !slotted.Value.IsVirtual)
        {
            return result.Passthrough();
        }

        // Block virtual using of skills but still fire off the action internally
        var skill = slotted.Value;
        if (skill.OnUse is not null)
        {
            Task.Run(() => skill.OnUse());
        }

        return result.Block();
    }

    private NetworkPacket? HandleClientCastSpellMessage(ProxyConnection connection, ClientCastSpellMessage message,
        object? parameter, NetworkMessageFilterResult<ClientCastSpellMessage> result)
    {
        if (!Spells.TryGetSlot(message.Slot, out var slotted) || !slotted.Value.IsVirtual)
        {
            return result.Passthrough();
        }

        // Block virtual using of spells but still fire off the action internally
        var spell = slotted.Value;
        if (spell.OnCast is not null)
        {
            var casterId = connection.UserId ?? 0;
            Task.Run(() =>
            {
                var parameters = spell.TargetType switch
                {
                    SpellTargetType.NoTarget => SpellCastParameters.NoTarget(casterId),
                    SpellTargetType.Target => SpellCastParameters.Target(casterId, message.TargetId ?? 0,
                        message.TargetX ?? 0, message.TargetY ?? 0),
                    SpellTargetType.Prompt =>
                        SpellCastParameters.WithTextInput(casterId, message.TextInput ?? string.Empty),
                    _ => SpellCastParameters.WithNumericInput(casterId, message.NumericInputs)
                };
                spell.OnCast(parameters);
            });
        }

        return result.Block();
    }

    private NetworkPacket HandleClientSwapSlotMessage(ProxyConnection connection, ClientSwapSlotMessage message,
        object? parameter, NetworkMessageFilterResult<ClientSwapSlotMessage> result)
    {
        var slotA = message.SourceSlot;
        var slotB = message.TargetSlot;

        if (message.Pane == ClientSlotSwapType.Inventory)
        {
            var hasItemA = Inventory.TryGetSlot(slotA, out var itemA);
            var hasItemB = Inventory.TryGetSlot(slotB, out var itemB);

            if (hasItemA && itemA.Value.IsVirtual)
            {
                _pendingVirtualItemSwaps[slotA] = (itemA.Value, slotB);
            }

            if (hasItemB && itemB.Value.IsVirtual)
            {
                _pendingVirtualItemSwaps[slotB] = (itemB.Value, slotA);
            }
        }
        else if (message.Pane == ClientSlotSwapType.Skills)
        {
            var hasSkillA = Skills.TryGetSlot(slotA, out var skillA);
            var hasSkillB = Skills.TryGetSlot(slotB, out var skillB);

            if (hasSkillA && skillA.Value.IsVirtual)
            {
                _pendingVirtualSkillSwaps[slotA] = (skillA.Value, slotB);
            }

            if (hasSkillB && skillB.Value.IsVirtual)
            {
                _pendingVirtualSkillSwaps[slotB] = (skillB.Value, slotA);
            }
        }
        else if (message.Pane == ClientSlotSwapType.Spells)
        {
            var hasSpellA = Spells.TryGetSlot(slotA, out var spellA);
            var hasSpellB = Spells.TryGetSlot(slotB, out var spellB);

            if (hasSpellA && spellA.Value.IsVirtual)
            {
                _pendingVirtualSpellSwaps[slotA] = (spellA.Value, slotB);
            }

            if (hasSpellB && spellB.Value.IsVirtual)
            {
                _pendingVirtualSpellSwaps[slotB] = (spellB.Value, slotA);
            }
        }

        return result.Passthrough();
    }

    private void HandleServerRemoveItemMessage(ProxyConnection connection, ServerRemoveItemMessage message,
        object? parameter)
    {
        var slot = message.Slot;
        if (!_pendingVirtualItemSwaps.TryRemove(slot, out var swap))
        {
            return;
        }

        var item = swap.Item;
        MoveVirtualItem(connection, item, swap.TargetSlot);
    }

    private void HandleServerRemoveSkillMessage(ProxyConnection connection, ServerRemoveSkillMessage message,
        object? parameter)
    {
        var slot = message.Slot;
        if (!_pendingVirtualSkillSwaps.TryRemove(slot, out var swap))
        {
            return;
        }

        var skill = swap.Skill;
        MoveVirtualSkill(connection, skill, swap.TargetSlot);
    }

    private void HandleServerRemoveSpellMessage(ProxyConnection connection, ServerRemoveSpellMessage message,
        object? parameter)
    {
        var slot = message.Slot;
        if (!_pendingVirtualSpellSwaps.TryRemove(slot, out var swap))
        {
            return;
        }

        var spell = swap.Spell;
        MoveVirtualSpell(connection, spell, swap.TargetSlot);
    }

    private void MoveVirtualItem(ProxyConnection connection, InventoryItem item, int targetSlot)
    {
        Inventory.SetSlot(targetSlot, item);

        var addItemMessage = new ServerAddItemMessage
        {
            Slot = (byte)targetSlot,
            Name = item.Name,
            Sprite = item.Sprite,
            Color = (DyeColor)item.Color,
            Durability = (uint)(item.Durability ?? 0),
            MaxDurability = (uint)(item.MaxDurability ?? 0),
            Quantity = (uint)item.Quantity,
            IsStackable = item.IsStackable
        };
        connection.EnqueueMessage(addItemMessage);
    }

    private void MoveVirtualSkill(ProxyConnection connection, SkillbookItem skill, int targetSlot)
    {
        Skills.SetSlot(targetSlot, skill);

        var addSkillMessage = new ServerAddSkillMessage
        {
            Slot = (byte)targetSlot,
            Name = skill.Name,
            Icon = skill.Sprite
        };

        connection.EnqueueMessage(addSkillMessage);
    }

    private void MoveVirtualSpell(ProxyConnection connection, SpellbookItem spell, int targetSlot)
    {
        Spells.SetSlot(targetSlot, spell);

        var addSpellMessage = new ServerAddSpellMessage
        {
            Slot = (byte)targetSlot,
            Name = spell.Name,
            Icon = spell.Sprite,
            TargetType = spell.TargetType,
            Prompt = spell.Prompt ?? string.Empty,
            CastLines = (byte)spell.CastLines
        };

        connection.EnqueueMessage(addSpellMessage);
    }
}