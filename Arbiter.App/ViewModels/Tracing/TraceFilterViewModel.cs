using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
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
            .Select(cmd => new CommandFilterViewModel(cmd));

        var serverCommandModels = Enum.GetValues<ServerCommand>()
            .OrderBy(cmd => cmd == ServerCommand.Unknown ? 1 : 0)
            .ThenBy(cmd => cmd.ToString())
            .Select(cmd => new CommandFilterViewModel(cmd));

        foreach (var vm in clientCommandModels)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CommandFilterViewModel.IsSelected))
                {
                    SelectedClientCommands = SelectedCommands.Select(x => x.Value).ToList();
                    OnPropertyChanged(nameof(SelectedClientCommands));
                    OnPropertyChanged(nameof(SelectedCommands));
                }
            };
            Commands.Add(vm);

            vm.IsSelected = true;
        }

        foreach (var vm in serverCommandModels)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CommandFilterViewModel.IsSelected))
                {
                    SelectedServerCommands = SelectedCommands.Select(x => x.Value).ToList();
                    OnPropertyChanged(nameof(SelectedServerCommands));
                    OnPropertyChanged(nameof(SelectedCommands));
                }
            };
            Commands.Add(vm);

            vm.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ClearNameFilter()
    {
        NameFilter = string.Empty;
    }
}