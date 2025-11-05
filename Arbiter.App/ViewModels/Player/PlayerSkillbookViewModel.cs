using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Arbiter.App.Collections;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerSkillbookViewModel : ViewModelBase
{
    private readonly ISlottedCollection<SkillbookItem> _skills;

    [ObservableProperty] private PlayerSkillSlotViewModel? _selectedSkill;

    public ObservableCollection<PlayerSkillSlotViewModel> TemuairSkills { get; } = [];
    public ObservableCollection<PlayerSkillSlotViewModel> MedeniaSkills { get; } = [];
    public ObservableCollection<PlayerSkillSlotViewModel> WorldSkills { get; } = [];

    public PlayerSkillbookViewModel(ISlottedCollection<SkillbookItem> skills)
    {
        _skills = skills;

        for (var i = 0; i < _skills.Capacity; i++)
        {
            if (i < 36)
            {
                TemuairSkills.Add(new PlayerSkillSlotViewModel(i + 1));
            }
            else if (i < 72)
            {
                MedeniaSkills.Add(new PlayerSkillSlotViewModel(i + 1));
            }
            else
            {
                WorldSkills.Add(new PlayerSkillSlotViewModel(i + 1));
            }
        }

        _skills.ItemAdded += OnSkillAdded;
        _skills.ItemRemoved += OnSkillRemoved;
    }

    public int? GetFirstEmptySlot(int startSlot = 1) => _skills.GetFirstEmptySlot(startSlot);

    public bool HasSkill(string name) => GetSkill(name, out _);

    public bool GetSkill(string name, [NotNullWhen(true)] out Slotted<SkillbookItem>? skill)
    {
        skill = null;
        if (!_skills.TryGetValue(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase),
                out var found))
        {
            return false;
        }

        skill = found;
        return true;
    }
    
    public bool TryGetSlot(int slot, out Slotted<SkillbookItem> skill)
    {
        skill = default;
        if (!_skills.TryGetValue(x => x.Slot == slot, out var found))
        {
            return false;
        }
        
        skill = found;
        return true;
    }

    public void SetSlot(int slot, SkillbookItem item) => _skills.SetSlot(slot, item);
    public void ClearSlot(int slot) => _skills.ClearSlot(slot);

    public bool TryRemoveSkill(string name, out int slot)
    {
        slot = 0;
        if (!_skills.TryGetValue(x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase),
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

    private void OnSkillAdded(Slotted<SkillbookItem> skill)
    {
        if (skill.Slot < 1 || skill.Slot > _skills.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSkillAdded(skill));
            return;
        }

        SetSkillViewModel(skill.Slot, skill.Value);
    }

    private void OnSkillRemoved(Slotted<SkillbookItem> skill)
    {
        if (skill.Slot < 1 || skill.Slot > _skills.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSkillRemoved(skill));
            return;
        }

        SetSkillViewModel(skill.Slot);
    }

    private void SetSkillViewModel(int slot, SkillbookItem? skill = null)
    {
        if (slot < 1 || slot > _skills.Capacity)
        {
            return;
        }

        var index = (slot - 1) % 36;

        switch (slot - 1)
        {
            case < 36:
                TemuairSkills[index] = new PlayerSkillSlotViewModel(slot, skill);
                break;
            case < 72:
                MedeniaSkills[index] = new PlayerSkillSlotViewModel(slot, skill);
                break;
            default:
                WorldSkills[index] = new PlayerSkillSlotViewModel(slot, skill);
                break;
        }
    }
}