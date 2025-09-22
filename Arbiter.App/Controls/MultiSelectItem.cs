using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Arbiter.App.Controls;

public class MultiSelectItem : ContentControl, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        ListBoxItem.IsSelectedProperty.AddOwner<MultiSelectItem>();

    public static readonly StyledProperty<IBrush?> CheckMarkBrushProperty =
        AvaloniaProperty.Register<MultiSelectItem, IBrush?>(nameof(CheckMarkBrush));

    // Back-reference to owning dropdown, set by MultiSelectDropdown during container prep
    public static readonly StyledProperty<MultiSelectDropdown?> OwnerProperty =
        AvaloniaProperty.Register<MultiSelectItem, MultiSelectDropdown?>(nameof(Owner));

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

    public MultiSelectDropdown? Owner
    {
        get => GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    static MultiSelectItem()
    {
        FocusableProperty.OverrideDefaultValue<MultiSelectItem>(true);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.Handled)
        {
            return;
        }

        // Ctrl+Click: select only this item and deselect others via parent dropdown
        if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
        {
            // Popup breaks the visual tree to the owner; prefer explicit Owner fallback
            var parent = Owner ?? this.FindAncestorOfType<MultiSelectDropdown>();

            if (parent is not null)
            {
                parent.SelectOnly(Content);
                e.Handled = true;
                return;
            }
        }
        
        // Default behavior: toggle selection
        IsSelected = !IsSelected;
        e.Handled = true;
    }
}