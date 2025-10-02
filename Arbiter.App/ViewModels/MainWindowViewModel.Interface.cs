using Arbiter.App.Models;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel
{
    private const double CollapsedWidth = 40;
    private const double CollapsedHeight = 40;

    // Left panel sizing state
    [ObservableProperty] private bool _isLeftPanelCollapsed;
    [ObservableProperty] private int _selectedLeftPanelTabIndex;
    [ObservableProperty] private GridLength _leftPanelWidth = new(1, GridUnitType.Star);
    [ObservableProperty] private double _leftPanelMinWidth = 240;
    [ObservableProperty] private double _leftPanelMaxWidth = 360;
    [ObservableProperty] private GridLength _savedLeftPanelWidth = new(1, GridUnitType.Star);

    // Right panel sizing state
    [ObservableProperty] private bool _isRightPanelCollapsed;
    [ObservableProperty] private int _selectedRightPanelTabIndex;
    [ObservableProperty] private GridLength _rightPanelWidth = new(1, GridUnitType.Star);
    [ObservableProperty] private double _rightPanelMinWidth = 260;
    [ObservableProperty] private double _rightPanelMaxWidth = 480;
    [ObservableProperty] private GridLength _savedRightPanelWidth = new(1, GridUnitType.Star);

    // Bottom panel sizing state
    [ObservableProperty] private bool _isBottomPanelCollapsed;
    [ObservableProperty] private int _selectedBottomPanelTabIndex;
    [ObservableProperty] private GridLength _bottomPanelHeight = new(1, GridUnitType.Star);
    [ObservableProperty] private double _bottomPanelMinHeight = 300;
    [ObservableProperty] private double _bottomPanelMaxHeight = 720;
    [ObservableProperty] private GridLength _savedBottomPanelHeight = new(1, GridUnitType.Star);

    [RelayCommand]
    private void ToggleLeftPanel(string? tabName)
    {
        var isNowVisible = IsLeftPanelCollapsed;
        if (isNowVisible)
        {
            // Panel is being expanded - restore the saved width
            LeftPanelWidth = SavedLeftPanelWidth;
            LeftPanelMinWidth = 240;
            LeftPanelMaxWidth = 360;

            // Set the selected tab if specified
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedLeftPanelTabIndex = tabName switch
                {
                    "hex" => 1,
                    _ => 0
                };
            }

            IsLeftPanelCollapsed = false;
        }
        else
        {
            CollapseLeftPanel();
        }
    }

    private void CollapseLeftPanel()
    {
        IsLeftPanelCollapsed = true;

        // Panel is being collapsed - save current width and set to collapsed size
        SavedLeftPanelWidth = LeftPanelWidth;
        LeftPanelWidth = new GridLength(CollapsedWidth);
        LeftPanelMinWidth = CollapsedWidth;
        LeftPanelMaxWidth = CollapsedWidth;
    }

    [RelayCommand]
    private void ToggleRightPanel(string? tabName)
    {
        var isNowVisible = IsRightPanelCollapsed;
        if (isNowVisible)
        {
            // Panel is being expanded - restore the saved width
            RightPanelWidth = SavedRightPanelWidth;
            RightPanelMinWidth = 240;
            RightPanelMaxWidth = 480;

            // Set the selected tab if specified
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedRightPanelTabIndex = tabName switch
                {
                    "hex" => 1,
                    "crc" => 2,
                    _ => 0
                };
            }

            IsRightPanelCollapsed = false;
        }
        else
        {
            CollapseRightPanel();
        }
    }

    private void CollapseRightPanel()
    {
        IsRightPanelCollapsed = true;

        // Panel is being collapsed - save current width and set to collapsed size
        SavedRightPanelWidth = RightPanelWidth;
        RightPanelWidth = new GridLength(CollapsedWidth);
        RightPanelMinWidth = CollapsedWidth;
        RightPanelMaxWidth = CollapsedWidth;
    }

    [RelayCommand]
    private void ToggleBottomPanel(string? tabName)
    {
        var isNowVisible = IsBottomPanelCollapsed;
        if (isNowVisible)
        {
            // Panel is being expanded - restore the saved height
            BottomPanelHeight = SavedBottomPanelHeight;
            BottomPanelMinHeight = 240;
            BottomPanelMaxHeight = 720;

            // Set the selected tab if specified
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedBottomPanelTabIndex = tabName switch
                {
                    "console" => 1,
                    _ => 0
                };
            }

            IsBottomPanelCollapsed = false;
        }
        else
        {
            CollapseBottomPanel();
        }
    }

    private void CollapseBottomPanel()
    {
        IsBottomPanelCollapsed = true;

        // Panel is being collapsed - save current height and set to collapsed size
        SavedBottomPanelHeight = BottomPanelHeight;
        BottomPanelHeight = new GridLength(CollapsedHeight);
        BottomPanelMinHeight = CollapsedHeight;
        BottomPanelMaxHeight = CollapsedHeight;
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

    private void RestoreWindowPosition()
    {
        var rect = Settings.StartupLocation;

        if (rect is null)
        {
            return;
        }

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

    private void SaveLayout()
    {
        Settings.LeftPanel = new InterfacePanelState
        {
            IsCollapsed = IsLeftPanelCollapsed,
            Width = LeftPanelWidth.Value > 10 ? LeftPanelWidth.Value : null
        };

        Settings.RightPanel = new InterfacePanelState
        {
            IsCollapsed = IsRightPanelCollapsed,
            Width = RightPanelWidth.Value > 10 ? RightPanelWidth.Value : null
        };

        Settings.BottomPanel = new InterfacePanelState
        {
            IsCollapsed = IsBottomPanelCollapsed,
            Height = BottomPanelHeight.Value > 10 ? BottomPanelHeight.Value : null
        };
    }

    private void RestoreLayout()
    {
        if (Settings.LeftPanel is not null)
        {
            if (Settings.LeftPanel.Width.HasValue)
            {
                if (Settings.LeftPanel.IsCollapsed)
                {
                    CollapseLeftPanel();
                }

                LeftPanelWidth = new GridLength(Settings.LeftPanel.Width.Value);
            }
        }

        if (Settings.RightPanel is not null)
        {
            if (Settings.RightPanel.IsCollapsed)
            {
                CollapseRightPanel();
            }

            if (Settings.RightPanel.Width.HasValue)
            {
                RightPanelWidth = new GridLength(Settings.RightPanel.Width.Value);
            }
        }

        if (Settings.BottomPanel is not null)
        {
            if (Settings.BottomPanel.IsCollapsed)
            {
                CollapseBottomPanel();
            }

            if (Settings.BottomPanel.Height.HasValue)
            {
                BottomPanelHeight = new GridLength(Settings.BottomPanel.Height.Value);
            }
        }
    }
}