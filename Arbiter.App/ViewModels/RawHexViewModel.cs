using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class RawHexViewModel : ViewModelBase
{
    private readonly IReadOnlyCollection<byte> _rawData;

    private Range? _selectedRange;
    private int _selectionStart;
    private int _selectionEnd;
    
    [ObservableProperty] private string _rawHex;
    
    public int SelectionStart
    {
        get => _selectionStart;
        set
        {
            if (SetProperty(ref _selectionStart, value))
            {
                RecalculateSelection();
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
            }
        }
    }
    
    public RawHexViewModel(IReadOnlyCollection<byte> rawData)
    {
        _rawData = rawData;
        RawHex = string.Join(" ", _rawData.Select(b => b.ToString("X2")));
    }

    private void RecalculateSelection()
    {
        _selectedRange = new Range(SelectionStart / 3, (SelectionEnd + 1) / 3);
    }
}