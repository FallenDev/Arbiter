using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Arbiter.App.ViewModels.Tracing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arbiter.App.Views;

public partial class CommandDropdown : UserControl
{
    public static readonly StyledProperty<string> DisplayTextProperty = AvaloniaProperty.Register<CommandDropdown, string>(
        nameof(DisplayText), "None");

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<CommandDropdown, bool>(
        nameof(IsOpen));

    public static readonly StyledProperty<ObservableCollection<CommandFilterViewModel>> CommandsProperty = AvaloniaProperty.Register<CommandDropdown, ObservableCollection<CommandFilterViewModel>>(
        nameof(Commands), []);

    private Border? _dropdownBorder;
    
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
    
    public ObservableCollection<CommandFilterViewModel> Commands
    {
        get => GetValue(CommandsProperty);
        set => SetValue(CommandsProperty, value);
    }

    public CommandDropdown()
    {
        InitializeComponent();
        DataContext = this;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _dropdownBorder = this.FindControl<Border>("DropdownBorder");

        if (_dropdownBorder is null)
        {
            return;
        }
        
        _dropdownBorder.PointerEntered += OnPointerEntered;
        _dropdownBorder.PointerExited += OnPointerExited;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        _dropdownBorder?.Classes.Add("focused");
    }
    
    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _dropdownBorder?.Classes.Remove("focused");
    }

    private void OnDropdownButtonClick(object? sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
        UpdateFocusedState();        
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CommandsProperty)
        {
            if (change.OldValue is ObservableCollection<CommandFilterViewModel> oldCommands)
            {
                foreach (var item in oldCommands)
                {
                    item.PropertyChanged -= OnCommandStatePropertyChanged;
                }
            }

            if (change.NewValue is ObservableCollection<CommandFilterViewModel> newCommands)
            {
                foreach (var item in newCommands)
                {
                    item.PropertyChanged += OnCommandStatePropertyChanged;
                }
            }
        }
        else if (change.Property == IsOpenProperty)
        {
            UpdateFocusedState();
        }
    }

    private void OnCommandStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CommandFilterViewModel.IsSelected))
        {
            UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        if (Commands.Count == 0)
        {
            DisplayText = "None";
            return;
        }

        var selectedCount = Commands.Count(c => c.IsSelected);
        var totalCount = Commands.Count;

        DisplayText = selectedCount switch
        {
            0 => "None",
            _ when selectedCount == totalCount => "All",
            1 => Commands.First(c => c.IsSelected).DisplayName,
            _ => $"{selectedCount} Selected"
        };
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