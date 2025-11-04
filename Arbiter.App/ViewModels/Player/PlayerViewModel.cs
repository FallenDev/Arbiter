using System;
using Arbiter.App.Models.Player;
using Arbiter.Net.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Player;

public partial class PlayerViewModel : ViewModelBase
{
    private readonly PlayerState _player;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    private long? _entityId;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int _level;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int _abilityLevel;

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
    [NotifyPropertyChangedFor(nameof(FormatedHealthText))]
    private long _currentHealth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HealthPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedHealthPercent))]
    [NotifyPropertyChangedFor(nameof(FormatedHealthText))]
    private long _maxHealth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManaPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedManaPercent))]
    [NotifyPropertyChangedFor(nameof(FormatedManaText))]
    private long _currentMana;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManaPercent))]
    [NotifyPropertyChangedFor(nameof(BoundedManaPercent))]
    [NotifyPropertyChangedFor(nameof(FormatedManaText))]
    private long _maxMana;

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
    
    public string FormatedHealthText =>
        $"{FormatHealthManaValue(CurrentHealth)} / {FormatHealthManaValue(MaxHealth)}";

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

    public string FormatedManaText =>
        $"{FormatHealthManaValue(CurrentMana)} / {FormatHealthManaValue(MaxMana)}";

    public PlayerInventoryViewModel Inventory { get; }
    public PlayerSkillbookViewModel Skillbook { get; }
    public PlayerSpellbookViewModel Spellbook { get; }

    public PlayerViewModel(PlayerState player)
    {
        _player = player;
        
        Inventory = new PlayerInventoryViewModel(player.Inventory);
        Skillbook = new PlayerSkillbookViewModel(player.Skillbook);
        Spellbook = new PlayerSpellbookViewModel(player.Spellbook);
    }

    public void Subscribe(ProxyConnection connection)
    {
        AddObservers(connection);
        AddVirtualFilters(connection);
    }

    public void Unsubscribe()
    {
        RemoveObservers();
        RemoveVirtualFilters();
    }

    // Forward all property changes to the player model
    partial void OnEntityIdChanged(long? value) => _player.UserId = value;
    partial void OnLevelChanged(int value) => _player.Level = value;
    partial void OnAbilityLevelChanged(int value) => _player.AbilityLevel = value;
    partial void OnClassChanged(string? value) => _player.Class = value;
    partial void OnMapIdChanged(int? value) => _player.MapId = value;
    partial void OnMapNameChanged(string? value) => _player.MapName = value;
    partial void OnMapXChanged(int? value) => _player.MapX = value;
    partial void OnMapYChanged(int? value) => _player.MapY = value;
    partial void OnCurrentHealthChanged(long value) => _player.CurrentHealth = value;
    partial void OnMaxHealthChanged(long value) => _player.MaxHealth = value;
    partial void OnCurrentManaChanged(long value) => _player.CurrentMana = value;
    partial void OnMaxManaChanged(long value) => _player.MaxMana = value;

    private static string FormatHealthManaValue(long value)
    {
        return value switch
        {
            < 1_000 => value.ToString(),
            < 1_000_000 => $"{(value / 1_000.0):0.0}k",
            _ => $"{(value / 1_000_000.0):0.0}m"
        };
    }
}