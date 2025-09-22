using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public static IReadOnlyList<PacketDirection> AvailablePacketDirections =>
        [PacketDirection.Client, PacketDirection.Server, PacketDirection.Both];

    
    private string _nameFilter = string.Empty;
    private string _commandFilter = string.Empty;
    
    [ObservableProperty] private IReadOnlyList<string> _nameFilterPatterns = [];
    
    public ObservableCollection<CommandFilterViewModel> ClientCommands { get; } = [];
    public ObservableCollection<CommandFilterViewModel> ServerCommands { get; } = [];

    public IEnumerable<CommandFilterViewModel> SelectedClientCommands => ClientCommands.Where(x => x.IsSelected);
    public IEnumerable<CommandFilterViewModel> SelectedServerCommands => ServerCommands.Where(x => x.IsSelected);
    
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
        var clientCommandModels = Enum.GetValues<ClientCommand>()
            .OrderBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, true));

        var serverCommandModels = Enum.GetValues<ServerCommand>()
            .OrderBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd, true));

        foreach (var vm in clientCommandModels)
        {
            vm.PropertyChanged += (_, _) => OnPropertyChanged(nameof(SelectedClientCommands));
            ClientCommands.Add(vm);
        }

        foreach (var vm in serverCommandModels)
        {
            vm.PropertyChanged += (_, _) => OnPropertyChanged(nameof(SelectedServerCommands));
            ServerCommands.Add(vm);
        }
    }

    [RelayCommand]
    private void ClearNameFilter()
    {
        NameFilter = string.Empty;
    }
}