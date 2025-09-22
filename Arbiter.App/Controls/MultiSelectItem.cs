using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Arbiter.App.Controls;

public class MultiSelectItem : ContentControl, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        ListBoxItem.IsSelectedProperty.AddOwner<MultiSelectItem>();

    protected override Type StyleKeyOverride => typeof(MultiSelectItem);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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