using System;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel : ViewModelBase
{
    public int Id { get; init; }
    public required string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    private long? _entityId;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int? _level;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLevel))]
    private int? _abilityLevel;

    [ObservableProperty] private string? _class;
    
    [ObservableProperty]
    private string? _mapName;
    
    [ObservableProperty]
    private int? _mapId;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapPosition))]
    private int? _mapX;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapPosition))]
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

    private readonly ProxyConnection _connection;

    public PlayerState Player { get; }

    public ClientViewModel(ProxyConnection connection, PlayerState state)
    {
        _connection = connection;
        Player = state;
    }

    public event EventHandler? BringToFrontRequested;

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
    
    public void Subscribe() => AddPacketFilters();
    public void Unsubscribe() => RemovePacketFilters();
    
    public bool EnqueuePacket(NetworkPacket packet, NetworkPriority priority = NetworkPriority.Normal) => _connection.EnqueuePacket(packet, priority);
    public bool EnqueueMessage(IClientMessage packet, NetworkPriority priority = NetworkPriority.Normal) => _connection.EnqueueMessage(packet, priority);
    public bool EnqueueMessage(IServerMessage packet, NetworkPriority priority = NetworkPriority.Normal) => _connection.EnqueueMessage(packet, priority);
    
    private static bool CanBringToFront() => OperatingSystem.IsWindows();

    [RelayCommand(CanExecute = nameof(CanBringToFront))]
    private void BringToFront()
    {
        BringToFrontRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanDisconnect() => _connection.IsConnected;

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public void Disconnect()
    {
        _connection.Disconnect();
        DisconnectCommand.NotifyCanExecuteChanged();
    }
}