using System;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.ViewModels;
using Arbiter.App.Views;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.App.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Window _mainWindow;

    public DialogService(IServiceProvider serviceProvider, Window mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
    }

    public Task ShowDialogAsync<TWindow>()
        where TWindow : Window, new()
    {
        var window = new TWindow();
        return window.ShowDialog(_mainWindow);
    }

    public Task ShowDialogAsync<TWindow, TViewModel>(TViewModel? viewModel = null)
        where TWindow : Window, new() where TViewModel : ViewModelBase
    {
        var window = new TWindow();
        var vm = viewModel ?? _serviceProvider.GetRequiredService<TViewModel>();

        window.DataContext = vm;
        return window.ShowDialog(_mainWindow);
    }

    public async Task<TResult?> ShowDialogAsync<TWindow, TViewModel, TResult>(TViewModel? viewModel = null)
        where TWindow : Window, new() where TViewModel : ViewModelBase, IDialogResult<TResult>
    {
        var window = new TWindow();
        var vm = viewModel ?? _serviceProvider.GetRequiredService<TViewModel>();

        window.DataContext = vm;

        Action<TResult?> handler = null!;
        TResult? dialogResult;

        try
        {
            handler = result => { window.Close(result); };

            vm.RequestClose += handler;
            dialogResult = await window.ShowDialog<TResult?>(_mainWindow);
        }
        finally
        {
            vm.RequestClose -= handler;
        }

        return dialogResult;
    }

    public Task<bool?> ShowMessageBoxAsync(MessageBoxDetails details)
    {
        var vm = new MessageBoxViewModel
        {
            Title = details.Title,
            Message = details.Message,
            Description = details.Description,
            Style = details.Style,
            AcceptButtonText = details.AcceptButtonText,
            CancelButtonText = details.CancelButtonText,
        };

        return ShowDialogAsync<MessageBoxWindow, MessageBoxViewModel, bool?>(vm);
    }
}