﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.Net.Proxy;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientManagerViewModel : ViewModelBase
{
    private readonly ProxyServer _proxyServer;
    private readonly IGameClientService _gameClientService;
    private readonly IPlayerService _playerService;
    private readonly ConcurrentDictionary<long, ClientViewModel> _clients = [];

    public ObservableCollection<ClientViewModel> Clients { get; } = [];
    
    [ObservableProperty]
    private ClientViewModel? _selectedClient;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasClients))]
    private int _clientCount;

    public bool HasClients => ClientCount > 0;
    
    public event Action<ClientViewModel?>? ClientSelected;
    public event Action<ClientViewModel>? ClientConnected;
    public event Action<ClientViewModel>? ClientDisconnected;

    public ClientManagerViewModel(ProxyServer proxyServer, IGameClientService gameClientService,
        IPlayerService playerService)
    {
        _proxyServer = proxyServer;
        _gameClientService = gameClientService;
        _playerService = playerService;

        _proxyServer.ClientAuthenticated += OnClientAuthenticated;
        _proxyServer.ClientLoggedIn += OnClientLoggedIn;
        _proxyServer.ClientLoggedOut += OnClientLoggedOut;
        _proxyServer.ClientRedirected += OnClientRedirected;
        _proxyServer.ClientDisconnected += OnClientDisconnected;
    }

    public bool TryGetClient(long id, [NotNullWhen(true)] out ClientViewModel? client) =>
        _clients.TryGetValue(id, out client);

    partial void OnSelectedClientChanged(ClientViewModel? oldValue, ClientViewModel? newValue)
    {
        ClientSelected?.Invoke(newValue);
    }

    private void OnClientAuthenticated(object? sender, ProxyConnectionEventArgs e)
    {
        var connection = e.Connection;
        var state = new PlayerState(connection.Id, connection.Name);
        var client = new ClientViewModel(connection, state)
            { Id = connection.Id, Name = connection.Name ?? string.Empty };

        // Start listening for packets and updating state
        client.Subscribe();

        _clients.AddOrUpdate(client.Id, client, (_, _) => client);
        _playerService.Register(connection.Id, state);

        ClientConnected?.Invoke(client);
    }

    private void OnClientLoggedIn(object? sender, ProxyConnectionEventArgs e)
    {
        var client = _clients.GetValueOrDefault(e.Connection.Id);

        if (client is null)
        {
            return;
        }

        var windowTitle = !string.IsNullOrWhiteSpace(e.Connection.Name)
            ? $"Darkages - {e.Connection.Name}"
            : "Darkages";

        SetClientWindowTitle(client, windowTitle);
        client.BringToFrontRequested += OnClientBringToFront;

        // We are fully logged in and can display this client in the list
        Dispatcher.UIThread.Post(() =>
        {
            Clients.Add(client);
            ClientCount = Clients.Count;
            
            // Select the client if no other selection
            SelectedClient ??= client;
        });
    }

    private void OnClientLoggedOut(object? sender, ProxyConnectionEventArgs e)
    {
        var client = _clients.GetValueOrDefault(e.Connection.Id);
        if (client is null)
        {
            return;
        }

        CleanupClient(client);
    }

    private void OnClientRedirected(object? sender, ProxyConnectionRedirectEventArgs e)
    {
        // Do nothing for now
    }

    private void OnClientDisconnected(object? sender, ProxyConnectionEventArgs e)
    {
        var client = Clients.FirstOrDefault(c => c.Id == e.Connection.Id);
        client?.Unsubscribe();

        if (client is null)
        {
            return;
        }

        CleanupClient(client);
    }

    private void CleanupClient(ClientViewModel client)
    {
        SetClientWindowTitle(client, "Darkages");
        client.BringToFrontRequested -= OnClientBringToFront;

        // Try removing the client from the list
        Dispatcher.UIThread.Post(() =>
        {
            Clients.Remove(client);
            ClientCount = Clients.Count;
        });

        _clients.TryRemove(client.Id, out _);
        _playerService.Unregister(client.Id);
        
        ClientDisconnected?.Invoke(client);
    }

    private void OnClientBringToFront(object? sender, EventArgs e)
    {
        if (sender is not ClientViewModel clientVm || !OperatingSystem.IsWindows())
        {
            return;
        }

        var clientWindow = _gameClientService.GetGameClients().FirstOrDefault(gc => gc.CharacterName == clientVm.Name);
        clientWindow?.BringToFront();
    }

    private void SetClientWindowTitle(ClientViewModel vm, string windowTitle)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var clientWindow = _gameClientService.GetGameClients().FirstOrDefault(gc => gc.CharacterName == vm.Name);
        clientWindow?.SetWindowTitle(windowTitle);
    }
}