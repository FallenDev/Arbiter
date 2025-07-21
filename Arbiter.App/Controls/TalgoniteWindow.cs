using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Arbiter.App.Controls;

public class TalgoniteWindow : Window
{
    private Grid _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;
    
    protected override Type StyleKeyOverride { get; } = typeof(TalgoniteWindow);
    
    public static readonly StyledProperty<Control> TitleBarContentProperty = AvaloniaProperty.Register<TalgoniteWindow, Control>(
        nameof(TitleBarContent));

    public Control TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty)
        {
            PseudoClasses.Set(":maximized", change.NewValue is WindowState.Maximized);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _titleBar = e.NameScope.Find<Grid>("PART_TitleBar")!;
        _titleBar.DoubleTapped += (_, _) => ToggleMaximizedState();
        
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton")!;
        _minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        
        _maximizeButton = e.NameScope.Find<Button>("PART_MaximizeButton")!;
        _maximizeButton.Click += (_, _) => ToggleMaximizedState();
        
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton")!;
        _closeButton.Click += (_, _) => Close();
    }

    private void ToggleMaximizedState()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}