using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Arbiter.App.Controls;

[PseudoClasses(":dragging", ":resizing", ":maximized")]
public class TalgoniteWindow : Window
{
    private Control? _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;
    private Control? _resizeGrip;

    private bool _isMouseDown;
    private Point _mouseDownPosition;
    
    protected override Type StyleKeyOverride { get; } = typeof(TalgoniteWindow);
    
    public static readonly StyledProperty<Control> TitleBarContentProperty = AvaloniaProperty.Register<TalgoniteWindow, Control>(
        nameof(TitleBarContent));

    public static readonly StyledProperty<IBrush?> TitleBarBorderBrushProperty = AvaloniaProperty.Register<TalgoniteWindow, IBrush?>(
        nameof(TitleBarBorderBrush), Brushes.Transparent);

    public static readonly StyledProperty<Thickness> TitleBarBorderThicknessProperty = AvaloniaProperty.Register<TalgoniteWindow, Thickness>(
        nameof(TitleBarBorderThickness), new Thickness(0, 0, 0, 1));
    
    public static readonly StyledProperty<TextAlignment> TitleAlignmentProperty = AvaloniaProperty.Register<TalgoniteWindow, TextAlignment>(
        nameof(TitleAlignment));

    public Control TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }
    
    public IBrush? TitleBarBorderBrush
    {
        get => GetValue(TitleBarBorderBrushProperty);
        set => SetValue(TitleBarBorderBrushProperty, value);
    }
    
    public Thickness TitleBarBorderThickness
    {
        get => GetValue(TitleBarBorderThicknessProperty);
        set => SetValue(TitleBarBorderThicknessProperty, value);
    }
    
    public TextAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
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
        
        _titleBar = e.NameScope.Find<Control>("PART_TitleBar")!;
        _titleBar.DoubleTapped += (_, _) => ToggleMaximizedState();

        // Avoid resizing if double-tapping custom content
        var titleBarContent = e.NameScope.Find<Control>("PART_TitleContent");
        if (titleBarContent is not null)
        {
            titleBarContent.DoubleTapped += (_, tappedEventArgs) => tappedEventArgs.Handled = true;
        }

        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton")!;
        _minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        
        _maximizeButton = e.NameScope.Find<Button>("PART_MaximizeButton")!;
        _maximizeButton.Click += (_, _) => ToggleMaximizedState();
        
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton")!;
        _closeButton.Click += (_, _) => Close();

        _resizeGrip = e.NameScope.Find<Control>("PART_ResizeGrip");
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
        else if (_resizeGrip?.IsPointerOver is true && CanResize)
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
        if (!CanResize)
        {
            return;
        }
        
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}