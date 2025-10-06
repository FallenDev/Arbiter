using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Filters;

public partial class MessageFilterListViewModel : ViewModelBase, IDialogResult<List<MessageFilter>>
{
    private string _testInput = string.Empty;

    [ObservableProperty] private string _title = "Message Filters";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddFilterCommand))]
    private string _inputText = string.Empty;

    public ObservableCollection<MessageFilterViewModel> SelectedFilters { get; } = [];
    
    public string TestInput
    {
        get => _testInput;
        set
        {
            if (SetProperty(ref _testInput, value))
            {
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<MessageFilterViewModel> Filters { get; } = [];

    public event Action<List<MessageFilter>?>? RequestClose;

    [RelayCommand]
    private void HandleOk()
    {
        var newFilters = Filters.Select(x => new MessageFilter
        {
            Pattern = x.Pattern,
        }).ToList();

        RequestClose?.Invoke(newFilters);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }

    private bool CanAddFilter() => !string.IsNullOrWhiteSpace(InputText) && 
                                   TryParseRegex(InputText, out _) &&
                                   !HasPattern(InputText);

    [RelayCommand(CanExecute = nameof(CanAddFilter))]
    private void AddFilter()
    {
        if (!TryParseRegex(InputText, out var regex) || HasPattern(regex.ToString()))
        {
            return;
        }

        Filters.Add(new MessageFilterViewModel { Pattern = regex.ToString() });
    }

    private bool HasPattern(string pattern) => Filters.Any(x => x.Pattern == pattern);

    private static bool TryParseRegex(string pattern, [NotNullWhen(true)] out Regex? regex)
    {
        regex = null;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            regex = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

}