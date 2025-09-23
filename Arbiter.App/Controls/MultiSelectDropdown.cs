using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Arbiter.App.Threading;

namespace Arbiter.App.Controls;

[TemplatePart("PART_Popup", typeof(Popup), IsRequired = true)]
[PseudoClasses(":dropdownopen", ":pressed")]
public class MultiSelectDropdown : SelectingItemsControl
{
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MultiSelectDropdown, bool>(nameof(IsDropDownOpen));

    public static readonly StyledProperty<double> MaxDropdownHeightProperty = AvaloniaProperty.Register<MultiSelectDropdown, double>(
        nameof(MaxDropdownHeight), 300);

    public static readonly StyledProperty<string> SelectionTextProperty =
        AvaloniaProperty.Register<MultiSelectDropdown, string>(
            nameof(SelectionText),
            "None");

    public static readonly StyledProperty<IDataTemplate?> SelectionTextTemplateProperty =
        AvaloniaProperty.Register<MultiSelectDropdown, IDataTemplate?>(nameof(SelectionTextTemplate));

    private readonly List<INotifyPropertyChanged> _itemSubscriptions = [];
    private INotifyCollectionChanged? _currentCollection;
    private readonly Debouncer _selectionTextDebouncer = new(TimeSpan.FromMilliseconds(25), Dispatcher.UIThread);
    
    protected override Type StyleKeyOverride => typeof(MultiSelectDropdown);
    
    // Multi-select specific properties
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public double MaxDropdownHeight
    {
        get => GetValue(MaxDropdownHeightProperty);
        set => SetValue(MaxDropdownHeightProperty, value);
    }

    public string SelectionText
    {
        get => GetValue(SelectionTextProperty);
        private set => SetValue(SelectionTextProperty, value);
    }

    public IDataTemplate? SelectionTextTemplate
    {
        get => GetValue(SelectionTextTemplateProperty);
        set => SetValue(SelectionTextTemplateProperty, value);
    }
    
    static MultiSelectDropdown()
    {
        // Ensure the control starts in multiple selection mode so initial item selections aren't collapsed to a single one
        SelectionModeProperty.OverrideDefaultValue<MultiSelectDropdown>(SelectionMode.Multiple);

        // Reflect IsDropDownOpen into the :dropdownopen pseudo-class
        IsDropDownOpenProperty.Changed.AddClassHandler<MultiSelectDropdown>((o, e) =>
        {
            o.PseudoClasses.Set(":dropdownopen", o.IsDropDownOpen);
        });

        // React to ItemsSource changes to keep selection summary current even before popup is opened
        ItemsSourceProperty.Changed.AddClassHandler<MultiSelectDropdown>((o, e) =>
        {
            o.WireUpItemsFromItemsSource();
            o.UpdateSelectionText();
        });
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
            // Establish back-reference to owner (the dropdown)
            msi.Owner = this;

            // Initialize the container's visual selection state from the data item once
            var prop = item?.GetType().GetProperty("IsSelected", BindingFlags.Public | BindingFlags.Instance);
            if (prop?.PropertyType == typeof(bool))
            {
                msi.SetIsSelectedFromModel((bool)(prop.GetValue(item) ?? false));
            }

            // Bind the container's CheckMarkBrush for per-item customization
            TryBindCheckMarkBrush(msi, item);

            // Update summary text whenever a container becomes prepared
            RequestSelectionTextUpdate();
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
            // Clear values to avoid reusing stale values if container is recycled
            msi.ClearValue(MultiSelectItem.CheckMarkBrushProperty);
            msi.ClearValue(MultiSelectItem.OwnerProperty);
        }
        base.ClearContainerForItemOverride(container);
        RequestSelectionTextUpdate();
    }

    private static void TryBindCheckMarkBrush(MultiSelectItem msi, object? item)
    {
        if (item is null)
        {
            return;
        }

        var itemType = item.GetType();

        // Bind to a property named "CheckMarkBrush" if present on the data item; otherwise, leave unset for default styling
        var brushProp = itemType.GetProperty("CheckMarkBrush", BindingFlags.Public | BindingFlags.Instance);
        if (brushProp?.PropertyType != null && typeof(IBrush).IsAssignableFrom(brushProp.PropertyType))
        {
            msi.Bind(MultiSelectItem.CheckMarkBrushProperty, new Binding("CheckMarkBrush")
            {
                Mode = BindingMode.OneWay
            });
        }
        else
        {
            // No per-item brush provided: default checkmark to the item's foreground by binding to the container's Foreground
            msi.Bind(MultiSelectItem.CheckMarkBrushProperty, new Binding
            {
                Path = "Foreground",
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay
            });
        }
    }

    private void WireUpItemsFromItemsSource()
    {
        var source = ItemsSource ?? Items;
        UnwireCollectionAndItems();

        if (source is INotifyCollectionChanged incc)
        {
            _currentCollection = incc;
            incc.CollectionChanged += OnCollectionChanged;
        }

        // Subscribe to current items' property changes (IsSelected)
        foreach (var obj in source)
        {
            if (obj is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnItemPropertyChanged;
                _itemSubscriptions.Add(inpc);
            }
        }
    }

    private void UnwireCollectionAndItems()
    {
        if (_currentCollection is not null)
        {
            _currentCollection.CollectionChanged -= OnCollectionChanged;
            _currentCollection = null;
        }

        foreach (var inpc in _itemSubscriptions)
        {
            inpc.PropertyChanged -= OnItemPropertyChanged;
        }
        _itemSubscriptions.Clear();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            UnwireCollectionAndItems();
            WireUpItemsFromItemsSource();
            RequestSelectionTextUpdate();
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (var obj in e.OldItems)
            {
                if (obj is INotifyPropertyChanged oldInpc)
                {
                    oldInpc.PropertyChanged -= OnItemPropertyChanged;
                    _itemSubscriptions.Remove(oldInpc);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var obj in e.NewItems)
            {
                if (obj is INotifyPropertyChanged newInpc)
                {
                    newInpc.PropertyChanged += OnItemPropertyChanged;
                    _itemSubscriptions.Add(newInpc);
                }
            }
        }

        RequestSelectionTextUpdate();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsSelected")
        {
            // Sync container visual state to the data item selection change
            if (sender is not null)
            {
                var itemIsSelected = (bool?)sender.GetType().GetProperty("IsSelected")?.GetValue(sender) ?? false;
                SyncContainerSelectionToItem(sender, itemIsSelected);
            }
            RequestSelectionTextUpdate();
        }
    }

    private void SyncContainerSelectionToItem(object item, bool isSelected)
    {
        // Use the built-in container lookup to avoid linear scans
        if (ContainerFromItem(item) is MultiSelectItem msi)
        {
            msi.SetIsSelectedFromModel(isSelected);
        }
    }

    private void RequestSelectionTextUpdate()
    {
        _selectionTextDebouncer.Execute(UpdateSelectionText);
    }

    private void UpdateSelectionText()
    {
        var itemsEnumerable = (IEnumerable)(ItemsSource ?? Items);
        var totalCount = 0;
        var selectedCount = 0;

        foreach (var item in itemsEnumerable)
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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Set(":pressed", true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Set(":pressed", false);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Set(":pressed", false);
    }

    public void SelectOnly(object? dataItem)
    {
        if (dataItem is null)
        {
            return;
        }

        var items = ItemsSource ?? Items;
        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var shouldSelect = ReferenceEquals(item, dataItem);

            // Prefer setting selection on the data item if it exposes IsSelected
            var prop = item.GetType().GetProperty("IsSelected", BindingFlags.Public | BindingFlags.Instance);
            if (prop?.PropertyType == typeof(bool) && prop.CanWrite)
            {
                prop.SetValue(item, shouldSelect);
            }
            else if (item is MultiSelectItem msi)
            {
                msi.IsSelected = shouldSelect;
            }
        }

        RequestSelectionTextUpdate();
    }
}