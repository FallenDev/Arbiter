using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Arbiter.App.Threading;

namespace Arbiter.App.Controls;

[TemplatePart("PART_Popup", typeof(Popup), IsRequired = true)]
[PseudoClasses(":dropdownopen", ":pressed")]
public class MultiSelectDropdown : SelectingItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new VirtualizingStackPanel());
    
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
    private readonly Debouncer _selectionTextDebouncer = new(TimeSpan.FromMilliseconds(25), Dispatcher.UIThread);
    
    private Popup? _popup;
    private INotifyCollectionChanged? _currentCollection;
    private DateTime _lastKeyNavigationAt;
    private ScrollViewer? _popupScrollViewer;
    private int _lastMatchIndex = -1;
    
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
        ItemsPanelProperty.OverrideDefaultValue<MultiSelectDropdown>(DefaultPanel);
        IsTextSearchEnabledProperty.OverrideDefaultValue<MultiSelectDropdown>(true);
        FocusableProperty.OverrideDefaultValue<MultiSelectDropdown>(true);
        
        // Ensure the control starts in multiple selection mode so initial item selections aren't collapsed to a single one
        SelectionModeProperty.OverrideDefaultValue<MultiSelectDropdown>(SelectionMode.Multiple);

        // Reflect IsDropDownOpen into the :dropdownopen pseudo-class
        IsDropDownOpenProperty.Changed.AddClassHandler<MultiSelectDropdown>((o, e) =>
        {
            o.PseudoClasses.Set(":dropdownopen", o.IsDropDownOpen);
            if (!o.IsDropDownOpen)
            {
                o.ClearActiveMatchHighlight();
            }
        });

        // React to ItemsSource changes to keep selection summary current even before popup is opened
        ItemsSourceProperty.Changed.AddClassHandler<MultiSelectDropdown>((o, e) =>
        {
            o.WireUpItemsFromItemsSource();
            o.UpdateSelectionText();
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_popup is not null)
        {
            _popup.Opened -= PopupOnOpened;
        }
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup is not null)
        {
            _popup.Opened += PopupOnOpened;
        }
    }

    private void PopupOnOpened(object? sender, EventArgs e)
    {
        // Cache the ScrollViewer inside the popup and perform a gentle initial scroll
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                _popupScrollViewer ??= _popup?.Child
                    ?.GetVisualDescendants()
                    .OfType<ScrollViewer>()
                    .FirstOrDefault();

                // Clear any lingering highlight from previous sessions
                ClearActiveMatchHighlight();

                // Ensure first item is realized so further alignment operations work reliably
                ScrollIntoView(0);
            }
            catch
            {
                // ItemsControl.ScrollIntoView may throw if items aren't ready yet; ignore and let layout settle.
            }
        }, DispatcherPriority.Background);
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
                // Bind the container's ModelIsSelected to the data item's IsSelected for direct checkbox binding
                msi.Bind(MultiSelectItem.ModelIsSelectedProperty, new Binding("IsSelected") { Mode = BindingMode.TwoWay });
            }

            // Bind the container's check mark brush for per-item customization
            msi.Bind(MultiSelectItem.CheckMarkBrushProperty, new Binding
            {
                Path = "Foreground",
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay
            });

            // Provide text used by Avalonia's TextSearch for keyboard lookup
            var searchText = GetItemSearchText(item);
            if (!string.IsNullOrEmpty(searchText))
            {
                TextSearch.SetText(msi, searchText);
            }
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MultiSelectItem msi)
        {
            // Clear values to avoid reusing stale values if container is recycled
            msi.ClearValue(MultiSelectItem.CheckMarkBrushProperty);
            msi.ClearValue(MultiSelectItem.OwnerProperty);
            msi.ClearValue(MultiSelectItem.ModelIsSelectedProperty);
            // Reset selection visual to a safe default to avoid stale checked state during recycling
            msi.IsSelected = false;
            // Ensure no lingering match highlight on recycled containers
            msi.SetMatchHighlight(false);
        }
        base.ClearContainerForItemOverride(container);
    }

    private static string? GetItemSearchText(object? item)
    {
        if (item is null)
        {
            return null;
        }

        if (item is string s)
        {
            return s;
        }

        try
        {
            var type = item.GetType();

            var displayNameProp = type.GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
            if (displayNameProp?.CanRead == true)
            {
                var val = displayNameProp.GetValue(item);
                var text = val?.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            var nameProp = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            if (nameProp?.CanRead == true)
            {
                var val = nameProp.GetValue(item);
                var text = val?.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }
        catch
        {
            // ignore and fallback
        }

        return item.ToString();
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

        foreach (var item in _itemSubscriptions)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
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
                if (obj is INotifyPropertyChanged oldItem)
                {
                    oldItem.PropertyChanged -= OnItemPropertyChanged;
                    _itemSubscriptions.Remove(oldItem);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var obj in e.NewItems)
            {
                if (obj is INotifyPropertyChanged newItem)
                {
                    newItem.PropertyChanged += OnItemPropertyChanged;
                    _itemSubscriptions.Add(newItem);
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
        var itemsEnumerable = ItemsSource ?? Items;
        var totalCount = 0;
        var selectedCount = 0;
        object? firstSelected = null;
        
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
                if (selectedCount == 1)
                {
                    // Remember the first and only selected item (so far)
                    firstSelected = item;
                }
            }
        }

        if (totalCount == 0 || selectedCount == 0)
        {
            SelectionText = "None";
        }
        else if (selectedCount == 1)
        {
            // Show the DisplayName of the selected item (if available) or fall back to ToString()
            string? display = null;

            var dataForName = firstSelected;
            if (dataForName is MultiSelectItem container)
            {
                dataForName = container.Content ?? container;
            }

            if (dataForName is not null)
            {
                var dnProp = dataForName.GetType().GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (dnProp is not null && dnProp.CanRead)
                {
                    try
                    {
                        var value = dnProp.GetValue(dataForName);
                        display = value?.ToString();
                    }
                    catch
                    {
                        // ignore and fall back to ToString()
                    }
                }

                display ??= dataForName.ToString();
            }

            SelectionText = string.IsNullOrWhiteSpace(display) ? "1 Selected" : display;
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

    private void BringIndexToTop(int index)
    {
        try { ScrollIntoView(index); } catch { }
        Dispatcher.UIThread.Post(() =>
        {
            var sv = _popupScrollViewer ?? _popup?.Child?.GetVisualDescendants()?.OfType<ScrollViewer>()?.FirstOrDefault();
            if (sv is null)
            {
                return;
            }
            if (ContainerFromIndex(index) is Control container)
            {
                var pt = container.TranslatePoint(new Point(0, 0), sv);
                if (pt.HasValue)
                {
                    var offset = sv.Offset;
                    var targetY = offset.Y + pt.Value.Y;
                    sv.Offset = new Vector(offset.X, targetY);
                }
            }
        }, DispatcherPriority.Background);
    }

    private void ClearActiveMatchHighlight()
    {
        if (_lastMatchIndex >= 0)
        {
            if (ContainerFromIndex(_lastMatchIndex) is MultiSelectItem prev)
            {
                prev.SetMatchHighlight(false);
            }
            _lastMatchIndex = -1;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty)
        {
            var index = change.NewValue is int i ? i : (change.NewValue as int?) ?? -1;
            if (index >= 0 && IsDropDownOpen)
            {
                // Only auto-scroll to selection if it likely came from keyboard text search recently.
                var since = DateTime.UtcNow - _lastKeyNavigationAt;
                if (since.TotalMilliseconds <= 300)
                {
                    BringIndexToTop(index);
                    // Clear previous highlight if different
                    if (_lastMatchIndex >= 0 && _lastMatchIndex != index)
                    {
                        if (ContainerFromIndex(_lastMatchIndex) is MultiSelectItem prev)
                        {
                            prev.SetMatchHighlight(false);
                        }
                    }
                    // Highlight the new match persistently
                    if (ContainerFromIndex(index) is MultiSelectItem msi)
                    {
                        msi.SetMatchHighlight(true);
                        _lastMatchIndex = index;
                    }
                }
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
        {
            return;
        }

        var alt = (e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
        switch (e.Key)
        {
            case Key.F4:
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
                return;
            case Key.Down when alt:
                IsDropDownOpen = true;
                e.Handled = true;
                return;
            case Key.Up when alt:
                IsDropDownOpen = false;
                e.Handled = true;
                return;
            case Key.Space:
            case Key.Enter:
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
                return;
            case Key.Escape when IsDropDownOpen:
                IsDropDownOpen = false;
                e.Handled = true;
                return;
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        if (IsTextSearchEnabled)
        {
            _lastKeyNavigationAt = DateTime.UtcNow;
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