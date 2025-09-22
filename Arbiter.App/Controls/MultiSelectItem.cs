using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Arbiter.App.Controls;

public class MultiSelectItem : ContentControl, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        ListBoxItem.IsSelectedProperty.AddOwner<MultiSelectItem>();

    public static readonly StyledProperty<IBrush?> CheckMarkBrushProperty =
        AvaloniaProperty.Register<MultiSelectItem, IBrush?>(nameof(CheckMarkBrush));

    protected override Type StyleKeyOverride => typeof(MultiSelectItem);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public IBrush? CheckMarkBrush
    {
        get => GetValue(CheckMarkBrushProperty);
        set => SetValue(CheckMarkBrushProperty, value);
    }

    static MultiSelectItem()
    {
        FocusableProperty.OverrideDefaultValue<MultiSelectItem>(true);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (!e.Handled)
        {
            IsSelected = !IsSelected;
            e.Handled = true;
        }
    }
}