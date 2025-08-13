using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class RawHexViewModel : ViewModelBase
{
    private readonly List<byte> _rawData;

    private Range? _selectedRange;
    private int _selectionStart;
    private int _selectionEnd;
    
    [NotifyPropertyChangedFor(nameof(FormattedCommand))]
    [ObservableProperty] private byte _command;
    
    [NotifyPropertyChangedFor(nameof(FormattedSequence))]
    [ObservableProperty] private byte? _sequence;
    
    [ObservableProperty] private string _rawHex;

    public string FormattedCommand => $"0x{Command:X2}";
    public string? FormattedSequence => Sequence is not null ? $"0x{Sequence:X2}" : null;
    
    public int SelectionStart
    {
        get => _selectionStart;
        set
        {
            if (SetProperty(ref _selectionStart, value))
            {
                RecalculateSelection();
                RefreshValues();
            }
        }
    }
    
    public int SelectionEnd
    {
        get => _selectionEnd;
        set
        {
            if (SetProperty(ref _selectionEnd, value))
            {
                RecalculateSelection();
                RefreshValues();
            }
        }
    }

    public RawHexViewModel(IReadOnlyCollection<byte> rawData, byte command, byte? sequence = null)
    {
        _rawData = rawData.ToList();
        RawHex = string.Join(" ", _rawData.Select(b => b.ToString("X2")));

        Command = command;
        Sequence = sequence;
    }

    private void RecalculateSelection()
    {
        _selectedRange = new Range(SelectionStart / 3, (SelectionEnd + 1) / 3);
    }

    private void RefreshValues()
    {
        OnPropertyChanged(nameof(Command));
        OnPropertyChanged(nameof(Sequence));
    }
}