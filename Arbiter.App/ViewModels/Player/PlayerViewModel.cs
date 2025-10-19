using System;
using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    private long? _entityId;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int? _level;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int? _abilityLevel;

    [ObservableProperty] private string? _class;

    [ObservableProperty] private string? _mapName;

    [ObservableProperty] private int? _mapId;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MapPosition))]
    private int? _mapX;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MapPosition))]
    private int? _mapY;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HealthPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedHealthPercent))]
    private uint _currentHealth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HealthPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedHealthPercent))]
    private uint _maxHealth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManaPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedManaPercent))]
    private uint _currentMana;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManaPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedManaPercent))]
    private uint _maxMana;

    public bool IsLoggedIn => EntityId is not null;

    public string MapPosition => MapX.HasValue && MapY.HasValue ? $"{MapX}, {MapY}" : "?, ?";

    public string DisplayLevel =>
        AbilityLevel is > 0 ? $"AB {AbilityLevel}" : Level is > 0 ? $"Lv {Level}" : string.Empty;

    public double HealthPercent
    {
        get
        {
            var current = Math.Max(0, CurrentHealth);
            var max = Math.Max(1, MaxHealth);
            return Math.Round(current * 100.0 / max, 1, MidpointRounding.AwayFromZero);
        }
    }

    public double BoundedHealthPercent => Math.Clamp(HealthPercent, 0, 100);

    public double ManaPercent
    {
        get
        {
            var current = Math.Max(0, CurrentMana);
            var max = Math.Max(1, MaxMana);
            return Math.Round(current * 100.0 / max, 1, MidpointRounding.AwayFromZero);
        }
    }

    public double BoundedManaPercent => Math.Clamp(ManaPercent, 0, 100);

    public PlayerInventoryViewModel Inventory { get; }

    public PlayerViewModel(PlayerState player)
    {
        Inventory = new PlayerInventoryViewModel(player.Inventory);
    }

}