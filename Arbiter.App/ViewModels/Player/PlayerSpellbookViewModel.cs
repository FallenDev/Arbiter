using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerSpellbookViewModel : ViewModelBase
{
    private readonly PlayerSpellbook _spellbook;

    [ObservableProperty] private PlayerSpellSlotViewModel? _selectedSpell;

    public ObservableCollection<PlayerSpellSlotViewModel> TemuairSpells { get; } = [];
    public ObservableCollection<PlayerSpellSlotViewModel> MedeniaSpells { get; } = [];
    public ObservableCollection<PlayerSpellSlotViewModel> WorldSpells { get; } = [];

    public PlayerSpellbookViewModel(PlayerSpellbook spellbook)
    {
        _spellbook = spellbook;

        for (var i = 0; i < spellbook.Capacity; i++)
        {
            if (i < 36)
            {
                TemuairSpells.Add(new PlayerSpellSlotViewModel(i + 1));
            }
            else if (i < 72)
            {
                MedeniaSpells.Add(new PlayerSpellSlotViewModel(i + 1));
            }
            else
            {
                WorldSpells.Add(new PlayerSpellSlotViewModel(i + 1));
            }
        }
        
        _spellbook.ItemAdded += OnSpellAdded;
        _spellbook.ItemUpdated += OnSpellUpdated;
        _spellbook.ItemRemoved += OnSpellRemoved;
    }
    
    public bool TryGetSlot(int slot, [NotNullWhen(true)] out SpellbookItem? spell)
    {
        spell = null;
        
        if (slot < 1 || slot > _spellbook.Capacity)
        {
            return false;
        }

        spell = _spellbook.GetSlot(slot);
        return spell is not null;
    }
    
    public int? GetFirstEmptySlot(int startSlot = 1)
    {
        for (var i = startSlot; i <= _spellbook.Capacity; i++)
        {
            if (_spellbook.GetSlot(i) is null)
            {
                return i;
            }
        }

        return null;
    }
    
    public bool TryRemoveSpell(string name, [NotNullWhen(true)] out int? slot)
    {
        slot = null;
        
        for (var i = 1; i <= _spellbook.Capacity; i++)
        {
            var skill = _spellbook.GetSlot(i);
            if (!string.Equals(name, skill?.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            slot = i;
            _spellbook.ClearSlot(i);
            return true;
        }

        return false;
    }

    public void SetSlot(int slot, SpellbookItem spell)
    {
        var existing = _spellbook.GetSlot(slot);
        if (existing?.IsVirtual is true)
        {
            return;
        }
        
        _spellbook.SetSlot(slot, spell);
    }

    public void ClearSlot(int slot) =>
        _spellbook.ClearSlot(slot);

    public void UpdateCooldown(int slot, TimeSpan duration)
        => _spellbook.UpdateCooldown(slot, duration);

    private void OnSpellAdded(int slot, SpellbookItem skill)
    {
        if (slot < 1 || slot > _spellbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSpellAdded(slot, skill));
            return;
        }

        SetSpellViewModel(slot, skill);
    }

    private void OnSpellUpdated(int slot, SpellbookItem existing, SpellbookItem updated)
    {
        if (slot < 1 || slot > _spellbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSpellUpdated(slot, existing, updated));
            return;
        }

        SetSpellViewModel(slot, updated);
    }

    private void OnSpellRemoved(int slot, SpellbookItem item)
    {
        if (slot < 1 || slot > _spellbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSpellRemoved(slot, item));
            return;
        }

        SetSpellViewModel(slot);
    }

    private void SetSpellViewModel(int slot, SpellbookItem? spell = null)
    {
        if (slot < 1 || slot > _spellbook.Capacity)
        {
            return;
        }

        var index = (slot - 1) % 36;

        switch (slot - 1)
        {
            case < 36:
                TemuairSpells[index] = new PlayerSpellSlotViewModel(slot, spell);
                break;
            case < 72:
                MedeniaSpells[index] = new PlayerSpellSlotViewModel(slot, spell);
                break;
            default:
                WorldSpells[index] = new PlayerSpellSlotViewModel(slot, spell);
                break;
        }
    }
}