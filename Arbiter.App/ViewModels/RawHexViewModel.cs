using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels;

public partial class RawHexViewModel : ViewModelBase
{
    private readonly NetworkPacket _packet;
    private readonly byte[] _payload;

    private int _startIndex;
    private int _endIndex;
    private int _selectionStart;
    private int _selectionEnd;

    [NotifyPropertyChangedFor(nameof(FormattedCommand))] [ObservableProperty]
    private byte _command;

    [NotifyPropertyChangedFor(nameof(FormattedSequence))] [ObservableProperty]
    private byte? _sequence;

    [ObservableProperty] private string _rawHex;

    [ObservableProperty] private string _decodedText;

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

    public RawHexViewModel(NetworkPacket packet)
    {
        _packet = packet;
        _payload = packet.Data.ToArray();
        RawHex = string.Join(" ", _payload.Select(b => b.ToString("X2")));

        Command = packet.Command;
        Sequence = packet switch
        {
            ClientPacket clientPacket => clientPacket.Sequence,
            ServerPacket serverPacket => serverPacket.Sequence,
            _ => null
        };
    }

    private void RecalculateSelection()
    {
        var startIndex = SelectionStart / 3;
        var endIndex = (SelectionEnd + 1) / 3;

        // If selection is reversed, swap it for consistency
        if (startIndex > endIndex)
        {
            (startIndex, endIndex) = (endIndex, startIndex);
        }

        _startIndex = Math.Max(0, startIndex);
        _endIndex = Math.Min(endIndex, _payload.Length);
    }

    private void RefreshValues()
    {
        DecodedText = GetDecodedText();
    }

    private string GetDecodedText()
    {
        if (_startIndex == _endIndex)
        {
            return string.Empty;
        }
        
        var dataSpan = _payload[_startIndex.._endIndex];
        var buffer = ArrayPool<char>.Shared.Rent(dataSpan.Length + 1);

        try
        {
            var decodedLength = Encoding.ASCII.GetChars(dataSpan, buffer);
            if (decodedLength < 1)
            {
                return string.Empty;
            }
            
            for (var i = 0; i < decodedLength; i++)
            {
                buffer[i] = buffer[i] switch
                {
                    var c when char.IsLetterOrDigit(c) => c,
                    var c when char.IsPunctuation(c) => c,
                    var c when char.IsSymbol(c) => c,
                    _ => '.'
                };
            }

            return new string(buffer, 0, decodedLength);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    [RelayCommand]
    public void SelectAll()
    {
        SelectionStart = 0;
        SelectionEnd = RawHex.Length;
    }
}