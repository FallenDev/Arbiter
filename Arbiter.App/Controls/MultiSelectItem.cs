using System;
using System.Reflection;
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

    private bool _suppressModelSync;

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

    public void SetIsSelectedFromModel(bool value)
    {
        try
        {
            _suppressModelSync = true;
            IsSelected = value;
        }
        finally
        {
            _suppressModelSync = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != IsSelectedProperty)
        {
            return;
        }

        // When the container selection changes (e.g., via checkbox), propagate to the data item unless suppressed
        if (_suppressModelSync)
        {
            return;
        }

        var dataItem = Content;
        if (dataItem is null)
        {
            return;
        }

        var prop = dataItem.GetType().GetProperty("IsSelected", BindingFlags.Public | BindingFlags.Instance);
        if (prop?.PropertyType != typeof(bool) || !prop.CanWrite)
        {
            return;
        }

        var newObj = change.NewValue;
        var desired = newObj is bool b ? b : (newObj as bool?) ?? false;
        var current = (bool)(prop.GetValue(dataItem) ?? false);
        if (current != desired)
        {
            prop.SetValue(dataItem, desired);
        }
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
        
        // Default behavior: toggle the data item's selection without using SelectingItemsControl's selection state
        var data = Content;
        if (data is not null)
        {
            var prop = data.GetType().GetProperty("IsSelected");
            if (prop?.PropertyType == typeof(bool) && prop.CanRead && prop.CanWrite)
            {
                var current = (bool)(prop.GetValue(data) ?? false);
                prop.SetValue(data, !current);

                // Keep the container visual in sync for immediate feedback
                SetIsSelectedFromModel(!current);
            }
        }
        e.Handled = true;
    }
}