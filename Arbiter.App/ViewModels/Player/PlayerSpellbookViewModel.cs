using System;
using System.Collections.ObjectModel;
using Arbiter.App.Collections;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerSpellbookViewModel : ViewModelBase
{
    private readonly ISlottedCollection<SpellbookItem> _spells;

    [ObservableProperty] private PlayerSpellSlotViewModel? _selectedSpell;

    public ObservableCollection<PlayerSpellSlotViewModel> TemuairSpells { get; } = [];
    public ObservableCollection<PlayerSpellSlotViewModel> MedeniaSpells { get; } = [];
    public ObservableCollection<PlayerSpellSlotViewModel> WorldSpells { get; } = [];

    public PlayerSpellbookViewModel(ISlottedCollection<SpellbookItem> spells)
    {
        _spells = spells;

        for (var i = 0; i < _spells.Capacity; i++)
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

        _spells.ItemAdded += OnSpellAdded;
        _spells.ItemRemoved += OnSpellRemoved;
    }
    
    public int? GetFirstEmptySlot(int startSlot = 1) => _spells.GetFirstEmptySlot(startSlot);
    
    public bool HasSpell(string name) => GetSpell(name, out _);

    public bool GetSpell(string name, out Slotted<SpellbookItem> spell)
    {
        spell = default;
        if (!_spells.TryGetValue(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase),
                out var found))
        {
            return false;
        }

        spell = found;
        return true;
    }
    
    public bool TryGetSlot(int slot, out Slotted<SpellbookItem> spell)
    {
        spell = default;
        if (!_spells.TryGetValue(x => x.Slot == slot, out var found))
        {
            return false;
        }
        
        spell = found;
        return true;
    }
    
    public void SetSlot(int slot, SpellbookItem spell)
        => _spells.SetSlot(slot, spell);
    
    public void ClearSlot(int slot)
        => _spells.ClearSlot(slot);

    public bool TryRemoveSpell(string name, out int slot)
    {
        slot = 0;
        if (!_spells.TryGetValue(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase),
                out var found))
        {
            return false;
        }

        slot = found.Slot;
        return true;
    }

    public void UpdateCooldown(int slot, TimeSpan duration)
    {

    }

    private void OnSpellAdded(Slotted<SpellbookItem> spell)
    {
        if (spell.Slot < 1 || spell.Slot > _spells.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSpellAdded(spell));
            return;
        }

        SetSpellViewModel(spell.Slot, spell.Value);
    }

    private void OnSpellRemoved(Slotted<SpellbookItem> spell)
    {
        if (spell.Slot < 1 || spell.Slot > _spells.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSpellRemoved(spell));
            return;
        }

        SetSpellViewModel(spell.Slot);
    }

    private void SetSpellViewModel(int slot, SpellbookItem? spell = null)
    {
        if (slot < 1 || slot > _spells.Capacity)
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