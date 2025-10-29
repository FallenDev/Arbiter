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

    public ObservableCollection<PlayerSkillSlotViewModel> TemuairSkills { get; } = [];
    public ObservableCollection<PlayerSkillSlotViewModel> MedeniaSkills { get; } = [];
    public ObservableCollection<PlayerSkillSlotViewModel> WorldSkills { get; } = [];

    public PlayerSkillbookViewModel(PlayerSkillbook skillbook)
    {
        _skillbook = skillbook;

        for (var i = 0; i < skillbook.Capacity; i++)
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

        SetSkillViewModel(slot, skill);
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

        SetSkillViewModel(slot, updated);
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

        SetSkillViewModel(slot);
    }

    private void SetSkillViewModel(int slot, SkillbookItem? skill = null)
    {
        if (slot < 1 || slot > _skillbook.Capacity)
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