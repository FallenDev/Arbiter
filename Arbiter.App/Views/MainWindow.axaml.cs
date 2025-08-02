using System;
using Arbiter.App.Controls;
using Arbiter.App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Arbiter.App.Views;

public partial class MainWindow : TalgoniteWindow
{
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
        try
        {
            var canClose = await ViewModel.OnClosing(e.CloseReason);
            e.Cancel = !canClose;
        }
        catch
        {
            e.Cancel = false;
        }
    }
}