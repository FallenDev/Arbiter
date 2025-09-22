using System;
using Arbiter.App.Controls;
using Arbiter.App.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Arbiter.App.Views;

public partial class MainWindow : TalgoniteWindow
{
    private bool _canClose;
    
    private MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel)!;
        
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_OnOpened(object? sender, EventArgs e)
    {
        try
        {
            await ViewModel.OnOpened();
        }
        catch
        {
            // Do nothing
        }
    }

    private async void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.OnLoaded();
        }
        catch
        {
            // Do nothing
        }
    }

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_canClose)
        {
            return;
        }

        try
        {
            // Do not close the window immediately, wait for async cleanup tasks
            // If you do not do this, the application will exit prematurely
            e.Cancel = true;
            _canClose = await ViewModel.OnClosing(e.CloseReason);

            // Re-trigger closing the window which will now actually close
            if (_canClose)
            {
                Close();
            }
        }
        catch
        {
            // There was an issue, the window still needs to close
            _canClose = true;
            Close();
        }
    }
}