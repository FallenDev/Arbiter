using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Arbiter.App.Controls;

[TemplatePart("PART_Popup", typeof(Popup), IsRequired = true)]
[PseudoClasses(":dropdownopen", ":pressed")]
public class MultiSelectDropdown : SelectingItemsControl
{
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MultiSelectDropdown, bool>(nameof(IsDropDownOpen));


    public static readonly StyledProperty<string> SelectionTextProperty =
        AvaloniaProperty.Register<MultiSelectDropdown, string>(
            nameof(SelectionText), 
            "All");
    
    protected override Type StyleKeyOverride => typeof(MultiSelectDropdown);
    
    // Multi-select specific properties
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public string SelectionText
    {
        get => GetValue(SelectionTextProperty);
        private set => SetValue(SelectionTextProperty, value);
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        // Create a container that wraps the item in a checkable container
        return new MultiSelectItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not MultiSelectItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MultiSelectItem msi)
        {
            // Bind the container's IsSelected to the data item's IsSelected property (TwoWay)
            msi.Bind(MultiSelectItem.IsSelectedProperty, new Binding("IsSelected")
            {
                Mode = BindingMode.TwoWay
            });

            // Update summary text whenever a container becomes prepared
            UpdateSelectionText();

            // Track selection changes on the container to keep header text in sync
            msi.PropertyChanged += ContainerOnPropertyChanged;
        }
    }

    private void ContainerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MultiSelectItem.IsSelectedProperty)
        {
            UpdateSelectionText();
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MultiSelectItem msi)
        {
            // Clear binding/value to avoid reusing stale bindings if container is recycled
            msi.ClearValue(MultiSelectItem.IsSelectedProperty);
            msi.PropertyChanged -= ContainerOnPropertyChanged;
        }
        base.ClearContainerForItemOverride(container);
        UpdateSelectionText();
    }

    private void UpdateSelectionText()
    {
        var items = Items;
        var totalCount = 0;
        var selectedCount = 0;

        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            totalCount++;
            var isSelected = false;

            // Prefer reading from the data item's IsSelected if present
            var prop = item.GetType().GetProperty("IsSelected");
            if (prop?.PropertyType == typeof(bool))
            {
                isSelected = (bool)(prop.GetValue(item) ?? false);
            }
            else if (item is MultiSelectItem msi)
            {
                isSelected = msi.IsSelected;
            }

            if (isSelected)
            {
                selectedCount++;
            }
        }

        if (totalCount == 0 || selectedCount == 0)
        {
            SelectionText = "None";
        }
        else if (selectedCount == totalCount)
        {
            SelectionText = "All";
        }
        else
        {
            SelectionText = $"{selectedCount} Selected";
        }
    }
}