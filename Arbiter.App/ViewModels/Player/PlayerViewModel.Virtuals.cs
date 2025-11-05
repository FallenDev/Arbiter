using System.Threading.Tasks;
using Arbiter.App.Models.Player;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerViewModel
{
    private const string FilterPrefix = nameof(PlayerViewModel);
    
    private NetworkFilterRef? _virtualItemFilter;
    private NetworkFilterRef? _virtualSkillFilter;
    private NetworkFilterRef? _virtualSpellFilter;
    private NetworkFilterRef? _virtualSlotFilter;

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
    }

    private void RemoveVirtualFilters()
    {
        _virtualItemFilter?.Unregister();
        _virtualSkillFilter?.Unregister();
        _virtualSpellFilter?.Unregister();
        _virtualSlotFilter?.Unregister();
    }
    
    private NetworkPacket? HandleClientUseItemMessage(ProxyConnection connection, ClientUseItemMessage message,
        object? parameter, NetworkMessageFilterResult<ClientUseItemMessage> result)
    {
        if (!Inventory.TryGetSlot(message.Slot, out var item) || !item.IsVirtual)
        {
            return result.Passthrough();
        }

        // Block virtual using of items but still fire off the action internally
        if (item.OnUse is not null)
        {
            Task.Run(() => item.OnUse());
        }
        
        return result.Block();
    }

    private NetworkPacket? HandleClientUseSkillMessage(ProxyConnection connection, ClientUseSkillMessage message,
        object? parameter, NetworkMessageFilterResult<ClientUseSkillMessage> result)
    {
        if (!Skillbook.TryGetSlot(message.Slot, out var skill) || !skill.IsVirtual)
        {
            return result.Passthrough();
        }

        // Block virtual using of skills but still fire off the action internally
        if (skill.OnUse is not null)
        {
            Task.Run(() => skill.OnUse());
        }
        
        return result.Block();
    }

    private NetworkPacket? HandleClientCastSpellMessage(ProxyConnection connection, ClientCastSpellMessage message,
        object? parameter, NetworkMessageFilterResult<ClientCastSpellMessage> result)
    {
        if (!Spellbook.TryGetSlot(message.Slot, out var spell) || !spell.IsVirtual)
        {
            return result.Passthrough();
        }

        // Block virtual using of spells but still fire off the action internally
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
        return result.Passthrough();
    }

}