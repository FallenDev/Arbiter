using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceFilterViewModel : ViewModelBase
{
    [GeneratedRegex(@"^([a-z,\?\*]{1,13},?)+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NameFilterRegex();
    
    private string _nameFilter = string.Empty;
    
    [ObservableProperty] private IReadOnlyList<string> _nameFilterPatterns = [];
    
    public ObservableCollection<CommandFilterViewModel> Commands { get; } = [];

    public IEnumerable<CommandFilterViewModel> SelectedCommands => Commands.Where(x => x.IsSelected);

    public IEnumerable<byte> SelectedClientCommands { get; set; } = [];
    public IEnumerable<byte> SelectedServerCommands { get; set; } = [];
    
    public string NameFilter
    {
        get => _nameFilter;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && !NameFilterRegex().IsMatch(value))
            {
                throw new ValidationException("Invalid name filter");
            }

            if (!SetProperty(ref _nameFilter, value))
            {
                return;
            }

            NameFilterPatterns = value.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        }
    }

    public TraceFilterViewModel()
    {
        InitializeCommands();
    }
    
    private void InitializeCommands()
    {
        var clientCommandModels = Enum.GetValues<ClientCommand>()
            .OrderBy(cmd => cmd == ClientCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, isSelected: true));

        var serverCommandModels = Enum.GetValues<ServerCommand>()
            .OrderBy(cmd => cmd == ServerCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, isSelected: true));

        foreach (var vm in clientCommandModels)
        {
            vm.PropertyChanged += OnCommandFilterPropertyChanged;
            Commands.Add(vm);
        }

        foreach (var vm in serverCommandModels)
        {
            vm.PropertyChanged += OnCommandFilterPropertyChanged;
            Commands.Add(vm);
        }
        
        UpdateSelectedCommands();
    }

    private void OnCommandFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CommandFilterViewModel.IsSelected))
        {
            UpdateSelectedCommands();
            NotifyCommandsChanged();
        }
    }

    private void UpdateSelectedCommands()
    {
        SelectedClientCommands = SelectedCommands.Where(x => x.Direction == PacketDirection.Client).Select(x => x.Value)
            .ToList();
        SelectedServerCommands = SelectedCommands.Where(x => x.Direction == PacketDirection.Server).Select(x => x.Value)
            .ToList();
    }

    private void NotifyCommandsChanged()
    {
        OnPropertyChanged(nameof(SelectedClientCommands));
        OnPropertyChanged(nameof(SelectedServerCommands));
        OnPropertyChanged(nameof(SelectedCommands));
    }

    [RelayCommand]
    private void ClearNameFilter()
    {
        NameFilter = string.Empty;
    }
 
}