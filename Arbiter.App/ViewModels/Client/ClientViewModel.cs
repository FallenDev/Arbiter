using System;
using System.Net.Sockets;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel(ProxyConnection connection) : ViewModelBase
{
    public static ClientViewModel DesignInstance => new(new ProxyConnection(0, new TcpClient()))
    {
        EntityId = 0xFEEDBEEF,
        Name = "VeryLongName",
        Class = "Summoner",
        MapName = "Black Dragon Vestibule",
        Level = 99,
        AbilityLevel = 99,
        MapId = 99999,
        MapX = 100,
        MapY = 100,
        CurrentHealth = 123_456,
        MaxHealth = 234_789,
        CurrentMana = 123_456,
        MaxMana = 234_789,
    };
    
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
    
    public void Subscribe()
    {
        connection.PacketReceived += OnPacketReceived;
    }

    public void Unsubscribe()
    {
        connection.PacketReceived -= OnPacketReceived;
    }

    private void OnPacketReceived(object? sender, NetworkPacketEventArgs e)
    {
        if (e.Packet is ClientPacket clientPacket)
        {
            if (ClientMessageFactory.Default.TryCreate(clientPacket, out var message))
            {
                Dispatcher.UIThread.Post(() => HandleClientMessage(message));
            }
        }

        if (e.Packet is ServerPacket serverPacket)
        {
            if (ServerMessageFactory.Default.TryCreate(serverPacket, out var message))
            {
                Dispatcher.UIThread.Post(() => HandleServerMessage(message));
            }
        }
    }

    private void HandleClientMessage(IClientMessage message)
    {
        switch (message)
        {
            case ClientWalkMessage walkMessage:
                Walk(walkMessage.Direction);
                break;
        }
    }

    private void HandleServerMessage(IServerMessage message)
    {
        switch (message)
        {
            case ServerUserIdMessage userIdMessage:
                EntityId = userIdMessage.UserId;
                Class = userIdMessage.Class.ToString();
                break;
            case ServerMapInfoMessage mapInfoMessage:
                MapName = mapInfoMessage.Name;
                MapId = mapInfoMessage.MapId;
                break;
            case ServerMapLocationMessage mapLocationMessage:
                MapX = mapLocationMessage.X;
                MapY = mapLocationMessage.Y;
                break;
            case ServerSelfProfileMessage profileMessage:
                Class = profileMessage.DisplayClass;
                break;
            case ServerUpdateStatsMessage statsMessage:
                UpdateStats(statsMessage);
                break;
        }
    }

    private void Walk(WorldDirection direction)
    {
        MapX = direction switch
        {
            WorldDirection.Left => MapX - 1,
            WorldDirection.Right => MapX + 1,
            _ => MapX
        };

        MapY = direction switch
        {
            WorldDirection.Up => MapY - 1,
            WorldDirection.Down => MapY + 1,
            _ => MapY
        };
    }

    private void UpdateStats(ServerUpdateStatsMessage message)
    {
        if (message.Level.HasValue)
        {
            Level = message.Level.Value;
        }

        if (message.AbilityLevel.HasValue)
        {
            AbilityLevel = message.AbilityLevel.Value;
        }

        if (message.Health.HasValue)
        {
            CurrentHealth = message.Health.Value;
        }

        if (message.MaxHealth.HasValue)
        {
            MaxHealth = message.MaxHealth.Value;
        }

        if (message.Mana.HasValue)
        {
            CurrentMana = message.Mana.Value;
        }

        if (message.MaxMana.HasValue)
        {
            MaxMana = message.MaxMana.Value;
        }
    }
}