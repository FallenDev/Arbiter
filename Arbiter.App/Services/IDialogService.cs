using System;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.ViewModels;
using Avalonia.Controls;

namespace Arbiter.App.Services;

public interface IDialogResult<out TResult>
{
    event Action<TResult?> RequestClose;
}

public interface IDialogService
{
    public Task ShowDialogAsync<TWindow>()
        where TWindow : Window, new();
    
    Task ShowDialogAsync<TWindow, TViewModel>(TViewModel? viewModel = null)
        where TWindow : Window, new() where TViewModel : ViewModelBase;

    Task<TResult?> ShowDialogAsync<TWindow, TViewModel, TResult>(TViewModel? viewModel = null)
        where TWindow : Window, new() where TViewModel : ViewModelBase, IDialogResult<TResult>;
    
    Task<bool?> ShowMessageBoxAsync(MessageBoxDetails details);
}