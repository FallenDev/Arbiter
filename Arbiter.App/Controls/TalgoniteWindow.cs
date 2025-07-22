using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Arbiter.App.Controls;

[PseudoClasses(":dragging", ":resizing", ":maximized")]
public class TalgoniteWindow : Window
{
    private Grid? _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;
    private Grid? _resizeGrip;

    private bool _isMouseDown;
    private Point _mouseDownPosition;
    
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

        _resizeGrip = e.NameScope.Find<Grid>("PART_ResizeGrip");
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (_titleBar?.IsPointerOver is true)
        {
            _isMouseDown = true;
            _mouseDownPosition = e.GetPosition(this);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
                PseudoClasses.Set(":dragging", true);
                _isMouseDown = false;
            }
        }
        else if (_resizeGrip?.IsPointerOver is true)
        {
            PseudoClasses.Set(":resizing", true);
            BeginResizeDrag(WindowEdge.SouthEast, e);
        }
        else
        {
            PseudoClasses.Set(":resizing", false);
            PseudoClasses.Set(":dragging", false);
            _isMouseDown = false;
        }
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        PseudoClasses.Set(":resizing", false);
        PseudoClasses.Set(":dragging", false);
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isMouseDown)
        {
            PseudoClasses.Set(":resizing", false);
        }
        base.OnPointerMoved(e);
    }

    private void ToggleMaximizedState()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}