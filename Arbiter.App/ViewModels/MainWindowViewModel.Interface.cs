using System.Threading.Tasks;
using Arbiter.App.Models;
using Arbiter.App.Views;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel
{
    private const double CollapsedInspectorWidth = 40;
    private const double CollapsedBottomHeight = 40;

    // Left panel sizing state
    [ObservableProperty] private GridLength _leftPanelWidth = new(1, GridUnitType.Star);
    
    // Right panel sizing state
    [ObservableProperty] private bool _isInspectorPanelCollapsed;
    [ObservableProperty] private int _selectedInspectorTabIndex;
    [ObservableProperty] private GridLength _rightPanelWidth = new(1, GridUnitType.Star);
    [ObservableProperty] private double _rightPanelMinWidth = 240;
    [ObservableProperty] private double _rightPanelMaxWidth = 480;
    [ObservableProperty] private GridLength _savedRightPanelWidth = new(1, GridUnitType.Star);

    // Bottom panel sizing state
    [ObservableProperty] private bool _isBottomPanelCollapsed;
    [ObservableProperty] private int _selectedBottomTabIndex;
    [ObservableProperty] private GridLength _bottomPanelHeight = new(1, GridUnitType.Star);
    [ObservableProperty] private double _bottomPanelMinHeight = 300;
    [ObservableProperty] private double _bottomPanelMaxHeight = 720;
    [ObservableProperty] private GridLength _savedBottomPanelHeight = new(1, GridUnitType.Star);

    [RelayCommand]
    private void ToggleInspectorPanel(string? tabName)
    {
        var isNowVisible = IsInspectorPanelCollapsed;
        IsInspectorPanelCollapsed = !IsInspectorPanelCollapsed;

        if (isNowVisible)
        {
            // Panel is being expanded - restore the saved width
            RightPanelWidth = SavedRightPanelWidth;
            RightPanelMinWidth = 240;
            RightPanelMaxWidth = 480;

            // Set the selected tab if specified
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedInspectorTabIndex = tabName switch
                {
                    "hex" => 1,
                    _ => 0
                };
            }
        }
        else
        {
            // Panel is being collapsed - save current width and set to collapsed size
            SavedRightPanelWidth = RightPanelWidth;
            RightPanelWidth = new GridLength(CollapsedInspectorWidth);
            RightPanelMinWidth = CollapsedInspectorWidth;
            RightPanelMaxWidth = CollapsedInspectorWidth;
        }
    }
    
    [RelayCommand]
    private void ToggleBottomPanel(string? tabName)
    {
        var isNowVisible = IsBottomPanelCollapsed;
        IsBottomPanelCollapsed = !IsBottomPanelCollapsed;

        if (isNowVisible)
        {
            // Panel is being expanded - restore the saved height
            BottomPanelHeight = SavedBottomPanelHeight;
            BottomPanelMinHeight = 240;
            BottomPanelMaxHeight = 720;

            // Set the selected tab if specified
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedBottomTabIndex = tabName switch
                {
                    "console" => 1,
                    _ => 0
                };
            }
        }
        else
        {
            // Panel is being collapsed - save current height and set to collapsed size
            SavedBottomPanelHeight = BottomPanelHeight;
            BottomPanelHeight = new GridLength(CollapsedBottomHeight);
            BottomPanelMinHeight = CollapsedBottomHeight;
            BottomPanelMaxHeight = CollapsedBottomHeight;
        }
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        var newSettings =
            await _dialogService.ShowDialogAsync<SettingsWindow, SettingsViewModel, ArbiterSettings>();

        if (newSettings is null)
        {
            return;
        }

        Settings = newSettings;
        await _settingsService.SaveToFileAsync(Settings);
        LaunchClientCommand.NotifyCanExecuteChanged();
    }

    private void SaveWindowPosition()
    {
        Settings.StartupLocation = new WindowRect
        {
            X = _mainWindow.Position.X,
            Y = _mainWindow.Position.Y,
            Width = (int)_mainWindow.Width,
            Height = (int)_mainWindow.Height,
            IsMaximized = _mainWindow.WindowState == WindowState.Maximized
        };
    }

    private void SaveLayout()
    {
        
    }

    private void RestoreWindowPosition(WindowRect rect)
    {
        if (rect is { X: >= 0, Y: >= 0 })
        {
            _mainWindow.Position = new PixelPoint(rect.X.Value, rect.Y.Value);
        }

        if (rect.Width is > 0)
        {
            _mainWindow.Width = rect.Width.Value;
        }

        if (rect.Height is > 0)
        {
            _mainWindow.Height = rect.Height.Value;
        }

        _mainWindow.WindowState = rect.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    private void RestoreLayout()
    {
        
    }
}