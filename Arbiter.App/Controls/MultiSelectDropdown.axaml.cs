using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace Arbiter.App.Controls;

public partial class MultiSelectDropdown : UserControl
{
    public static readonly StyledProperty<string> DisplayTextProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(DisplayText), "None");

    public static readonly StyledProperty<bool> IsOpenProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, bool>(nameof(IsOpen));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<string> DisplayMemberPathProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(DisplayMemberPath), "");

    public static readonly StyledProperty<string> SelectedValuePathProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(SelectedValuePath), "");

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<string> NoneSelectedTextProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(NoneSelectedText), "None");

    public static readonly StyledProperty<string> AllSelectedTextProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(AllSelectedText), "All");

    public static readonly StyledProperty<string> MultipleSelectedFormatProperty = 
        AvaloniaProperty.Register<MultiSelectDropdown, string>(nameof(MultipleSelectedFormat), "{0} Selected");

    private Border? _dropdownBorder;
    private readonly List<object> _trackedItems = [];

    public string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string DisplayMemberPath
    {
        get => GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string SelectedValuePath
    {
        get => GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string NoneSelectedText
    {
        get => GetValue(NoneSelectedTextProperty);
        set => SetValue(NoneSelectedTextProperty, value);
    }

    public string AllSelectedText
    {
        get => GetValue(AllSelectedTextProperty);
        set => SetValue(AllSelectedTextProperty, value);
    }

    public string MultipleSelectedFormat
    {
        get => GetValue(MultipleSelectedFormatProperty);
        set => SetValue(MultipleSelectedFormatProperty, value);
    }

    public MultiSelectDropdown()
    {
        InitializeComponent();
        
        // Set default item template if none provided
        ItemTemplate = CreateDefaultItemTemplate();
    }

    private IDataTemplate CreateDefaultItemTemplate()
    {
        return new FuncDataTemplate<object>((item, _) =>
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                Margin = new Thickness(4, 0),
                Background = Avalonia.Media.Brushes.Transparent
            };
            
            var checkBox = new CheckBox();
            var textBlock = new TextBlock
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };

            // Bind checkbox to selected value
            if (!string.IsNullOrEmpty(SelectedValuePath))
            {
                checkBox.Bind(CheckBox.IsCheckedProperty, new Avalonia.Data.Binding(SelectedValuePath));
            }

            // Bind text to display value
            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(DisplayMemberPath));
            }
            else
            {
                textBlock.Text = item?.ToString() ?? string.Empty;
            }

            Grid.SetColumn(checkBox, 0);
            Grid.SetColumn(textBlock, 1);
            
            grid.Children.Add(checkBox);
            grid.Children.Add(textBlock);

            return grid;
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        _dropdownBorder = this.FindControl<Border>("DropdownBorder");
        if (_dropdownBorder is not null)
        {
            _dropdownBorder.PointerEntered += OnPointerEntered;
            _dropdownBorder.PointerExited += OnPointerExited;
        }

        UpdateDisplayText();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        if (_dropdownBorder is not null)
        {
            _dropdownBorder.PointerEntered -= OnPointerEntered;
            _dropdownBorder.PointerExited -= OnPointerExited;
        }
        
        UnsubscribeFromItems();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        _dropdownBorder?.Classes.Add("focused");
    }
    
    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _dropdownBorder?.Classes.Remove("focused");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            HandleItemsSourceChanged();
        }
        else if (change.Property == IsOpenProperty)
        {
            UpdateFocusedState();
        }
        else if (change.Property == ItemTemplateProperty && ItemTemplate == null)
        {
            ItemTemplate = CreateDefaultItemTemplate();
        }
    }

    private void HandleItemsSourceChanged()
    {
        UnsubscribeFromItems();
        SubscribeToItems();
        UpdateDisplayText();
    }

    private void SubscribeToItems()
    {
        if (ItemsSource == null || string.IsNullOrEmpty(SelectedValuePath))
        {
            return;
        }

        foreach (var item in ItemsSource)
        {
            if (item is not INotifyPropertyChanged notifyItem)
            {
                continue;
            }

            _trackedItems.Add(item);
            notifyItem.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void UnsubscribeFromItems()
    {
        foreach (var item in _trackedItems.OfType<INotifyPropertyChanged>())
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }
        _trackedItems.Clear();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == SelectedValuePath.Split('.').Last())
        {
            UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        if (ItemsSource == null)
        {
            DisplayText = NoneSelectedText;
            return;
        }

        var items = ItemsSource.Cast<object>().ToList();
        if (items.Count == 0)
        {
            DisplayText = NoneSelectedText;
            return;
        }

        var selectedCount = GetSelectedCount(items);
        var totalCount = items.Count;

        DisplayText = selectedCount switch
        {
            0 => NoneSelectedText,
            _ when selectedCount == totalCount => AllSelectedText,
            1 => GetDisplayText(GetFirstSelectedItem(items)),
            _ => string.Format(MultipleSelectedFormat, selectedCount)
        };
    }

    private int GetSelectedCount(IList<object> items)
    {
        return string.IsNullOrEmpty(SelectedValuePath)
            ? 0
            : items.Count(item => GetPropertyValue<bool>(item, SelectedValuePath));
    }

    private object? GetFirstSelectedItem(IList<object> items)
    {
        return string.IsNullOrEmpty(SelectedValuePath)
            ? null
            : items.FirstOrDefault(item => GetPropertyValue<bool>(item, SelectedValuePath));
    }

    private string GetDisplayText(object? item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(DisplayMemberPath))
        {
            return item.ToString() ?? string.Empty;
        }

        return GetPropertyValue<object>(item, DisplayMemberPath).ToString() ?? string.Empty;
    }

    private static T GetPropertyValue<T>(object obj, string propertyPath)
    {
        try
        {
            var value = obj;
            foreach (var propertyName in propertyPath.Split('.'))
            {
                if (value == null) return default!;
                
                var property = value.GetType().GetProperty(propertyName);
                if (property == null) return default!;
                
                value = property.GetValue(value);
            }
            
            return value is T result ? result : default!;
        }
        catch
        {
            return default!;
        }
    }

    private void UpdateFocusedState()
    {
        if (IsOpen)
        {
            _dropdownBorder?.Classes.Add("focused");
        }
        else
        {
            _dropdownBorder?.Classes.Remove("focused");
        }
    }
}