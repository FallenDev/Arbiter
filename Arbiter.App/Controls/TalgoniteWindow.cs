using System;
using Avalonia;
using Avalonia.Controls;

namespace Arbiter.App.Controls;

public class TalgoniteWindow : Window
{
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
}