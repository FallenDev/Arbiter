using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Arbiter.App.Models;
using Arbiter.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Filters;

public partial class MessageFilterListViewModel : ViewModelBase, IDialogResult<List<MessageFilter>>
{
    [ObservableProperty] private MessageFilterViewModel? _selectedFilter;

    [ObservableProperty] private string _title = "Message Filters";

    public ObservableCollection<MessageFilterViewModel> Filters { get; } = [];

    public event Action<List<MessageFilter>?>? RequestClose;

    [RelayCommand]
    private void HandleOk()
    {
        var newFilters = Filters.Select(x => new MessageFilter
        {
            Name = x.DisplayName,
            Pattern = x.Pattern,
        }).ToList();

        RequestClose?.Invoke(newFilters);
    }

    [RelayCommand]
    private void HandleCancel()
    {
        RequestClose?.Invoke(null);
    }
}