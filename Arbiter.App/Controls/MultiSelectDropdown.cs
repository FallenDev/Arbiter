using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

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
            "None");
    
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
}