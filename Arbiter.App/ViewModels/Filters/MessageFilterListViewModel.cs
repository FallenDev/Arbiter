using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Filters;

public partial class MessageFilterListViewModel : ViewModelBase, IDialogResult<List<MessageFilter>>
{
    private readonly Debouncer _testRecheckDebouncer = new(TimeSpan.FromMilliseconds(300), Dispatcher.UIThread);
    private string _testInput = string.Empty;

    [ObservableProperty] private bool _isTestInputAllowed;
    [ObservableProperty] private bool _isTestInputFiltered;

    [ObservableProperty] private string _title = "Message Filters";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddFilterCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyEditCommand))]
    private bool _isEditing;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddFilterCommand))]
    
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
                _testRecheckDebouncer.Execute(RecheckTestInput);
            }
        }
    }

    public ObservableCollection<MessageFilterViewModel> Filters { get; } = [];

    public event Action<List<MessageFilter>?>? RequestClose;

    public MessageFilterListViewModel()
    {
        // Stop editing when selection changes
        SelectedFilters.CollectionChanged += (_, _) =>
        {
            if (IsEditing)
            {
                InputText = string.Empty;
                IsEditing = false;
            }
        };
    }

    [RelayCommand]
    private void HandleOk()
    {
        var newFilters = Filters.Select(x => new MessageFilter
        {
            IsEnabled = x.IsEnabled,
            Pattern = x.Pattern,
        }).ToList();

        RequestClose?.Invoke(newFilters);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }

    private bool IsValidFilter() => !string.IsNullOrWhiteSpace(InputText) &&
                                    TryParseRegex(InputText, RegexOptions.IgnoreCase, out _);

    [RelayCommand(CanExecute = nameof(IsValidFilter))]
    private void AddFilter()
    {
        if (IsEditing ||
            !TryParseRegex(InputText, RegexOptions.IgnoreCase, out var regex))
        {
            return;
        }

        var filter = new MessageFilterViewModel { Pattern = regex.ToString() };
        filter.PropertyChanged += HandleItemChanged;

        Filters.Add(filter);
        InputText = string.Empty;
        
        _testRecheckDebouncer.Execute(RecheckTestInput);
    }

    private bool CanEdit() => SelectedFilters.Count == 1;
    
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void EditSelected()
    {
        if (SelectedFilters.Count != 1)
        {
            return;
        }

        InputText = SelectedFilters[0].Pattern;
        IsEditing = true;
    }

    private bool CanApplyEdit() => SelectedFilters.Count == 1 && IsValidFilter();

    [RelayCommand(CanExecute = nameof(CanApplyEdit))]
    private void ApplyEdit()
    {
        if (!IsEditing || SelectedFilters.Count != 1 ||
            !TryParseRegex(InputText, RegexOptions.IgnoreCase, out var regex))
        {
            return;
        }

        SelectedFilters[0].Pattern = regex.ToString();

        InputText = string.Empty;
        IsEditing = false;
    }

    private void RecheckTestInput()
    {
        if (string.IsNullOrWhiteSpace(TestInput))
        {
            IsTestInputAllowed = false;
            IsTestInputFiltered = false;
            return;
        }

        // If any enabled filter pattern matches the test input mark it as filtered
        var isFiltered = Filters.Any(f =>
            f.IsEnabled &&
            TryParseRegex(f.Pattern, RegexOptions.IgnoreCase, out var rx) && rx.IsMatch(TestInput));

        IsTestInputFiltered = isFiltered;
        IsTestInputAllowed = !isFiltered;
    }

    private static bool TryParseRegex(string pattern, RegexOptions options, [NotNullWhen(true)] out Regex? regex)
    {
        regex = null;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            regex = new Regex(pattern, options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool HasSelection() => SelectedFilters.Count > 0;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void EnableSelected()
    {
        if (SelectedFilters.Count == 0)
        {
            return;
        }

        foreach (var filter in SelectedFilters)
        {
            filter.IsEnabled = true;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DisableSelected()
    {
        if (SelectedFilters.Count == 0)
        {
            return;
        }

        foreach (var filter in SelectedFilters)
        {
            filter.IsEnabled = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteSelected()
    {
        if (SelectedFilters.Count == 0)
        {
            return;
        }

        var selectedFilters = SelectedFilters.ToList();
        foreach (var filter in selectedFilters)
        {
            filter.PropertyChanged -= HandleItemChanged;
            Filters.Remove(filter);
        }

        SelectedFilters.Clear();
        _testRecheckDebouncer.Execute(RecheckTestInput);

        AddFilterCommand.NotifyCanExecuteChanged();
        EditSelectedCommand.NotifyCanExecuteChanged();

        if (IsEditing)
        {
            InputText = string.Empty;
            IsEditing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanMoveSelectedUp))]
    private void MoveSelectedUp() => MoveSelectedItems(-1);

    private bool CanMoveSelectedUp()
    {
        if (SelectedFilters.Count == 0)
            return false;

        var selectedItems = SelectedFilters.ToList();
        var minIndex = selectedItems.Select(item => Filters.IndexOf(item))
            .Where(index => index >= 0)
            .DefaultIfEmpty(int.MaxValue)
            .Min();

        return minIndex > 0;
    }

    [RelayCommand(CanExecute = nameof(CanMoveSelectedDown))]
    private void MoveSelectedDown() => MoveSelectedItems(1);

    private bool CanMoveSelectedDown()
    {
        if (SelectedFilters.Count == 0)
            return false;

        var selectedItems = SelectedFilters.ToList();
        var maxIndex = selectedItems.Select(item => Filters.IndexOf(item))
            .Where(index => index >= 0)
            .DefaultIfEmpty(-1)
            .Max();

        return maxIndex < Filters.Count - 1;
    }

    private void MoveSelectedItems(int offset)
    {
        if (SelectedFilters.Count == 0)
            return;

        var selectedItems = SelectedFilters.ToList();
        var indicesToMove = selectedItems.Select(item => Filters.IndexOf(item))
            .Where(index => index >= 0)
            .OrderBy(x => offset > 0 ? -x : x)
            .ToList();

        if (indicesToMove.Count == 0)
            return;

        foreach (var index in indicesToMove)
        {
            var newIndex = index + offset;
            if (newIndex < 0 || newIndex >= Filters.Count)
                continue;

            var item = Filters[index];
            Filters.RemoveAt(index);
            Filters.Insert(newIndex, item);
        }

        SelectedFilters.Clear();
        foreach (var item in selectedItems)
        {
            SelectedFilters.Add(item);
        }
    }

    private void HandleItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        _testRecheckDebouncer.Execute(RecheckTestInput);
    }

    [RelayCommand]
    private static void VisitRegex101()
    {
        Process.Start(new ProcessStartInfo("https://regex101.com/") { UseShellExecute = true });
    }
}