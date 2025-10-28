using System;
using System.Collections.ObjectModel;
using Arbiter.App.Models.Player;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerSkillbookViewModel : ViewModelBase
{
    private readonly PlayerSkillbook _skillbook;

    [ObservableProperty] private PlayerSkillSlotViewModel? _selectedSkill;

    public ObservableCollection<PlayerSkillSlotViewModel> SkillSlots { get; } = [];

    public PlayerSkillbookViewModel(PlayerSkillbook skillbook)
    {
        _skillbook = skillbook;

        for (var i = 0; i < skillbook.Capacity; i++)
        {
            SkillSlots.Add(new PlayerSkillSlotViewModel(i + 1));
        }

        _skillbook.ItemAdded += OnSkillAdded;
        _skillbook.ItemUpdated += OnSkillUpdated;
        _skillbook.ItemRemoved += OnSkillRemoved;
    }

    public void SetSlot(int slot, SkillbookItem skill) =>
        _skillbook.SetSlot(slot, skill);

    public void ClearSlot(int slot) =>
        _skillbook.ClearSlot(slot);

    public void UpdateCooldown(int slot, TimeSpan duration)
        => _skillbook.UpdateCooldown(slot, duration);
    
    private void OnSkillAdded(int slot, SkillbookItem skill)
    {
        if (slot < 1 || slot > _skillbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSkillAdded(slot, skill));
            return;
        }

        SkillSlots[slot - 1] = new PlayerSkillSlotViewModel(slot, skill);
    }

    private void OnSkillUpdated(int slot, SkillbookItem existing, SkillbookItem updated)
    {
        if (slot < 1 || slot > _skillbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSkillUpdated(slot, existing, updated));
            return;
        }

        SkillSlots[slot - 1] = new PlayerSkillSlotViewModel(slot, updated);
    }

    private void OnSkillRemoved(int slot, SkillbookItem item)
    {
        if (slot < 1 || slot > _skillbook.Capacity)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSkillRemoved(slot, item));
            return;
        }

        SkillSlots[slot - 1] = new PlayerSkillSlotViewModel(slot);
    }
}