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
    private readonly byte[] _payload;
    
    private bool _showValuesAsHex;
    private int _startIndex;
    private int _endIndex;
    private int _hexSelectionStart;
    private int _hexSelectionEnd;
    private int _textSelectionStart;
    private int _textSelectionEnd;
    private bool _isSyncing;

    [NotifyPropertyChangedFor(nameof(FormattedCommand))] [ObservableProperty]
    private byte _command;

    [NotifyPropertyChangedFor(nameof(FormattedSequence))] [ObservableProperty]
    private byte? _sequence;

    [ObservableProperty] private string _rawHex;
    [ObservableProperty] private string _decodedText;
    [ObservableProperty] private string? _formattedSignedByte;
    [ObservableProperty] private string? _formattedUnsignedByte;
    [ObservableProperty] private string? _formattedSignedShort;
    [ObservableProperty] private string? _formattedUnsignedShort;
    [ObservableProperty] private string? _formattedSignedInt;
    [ObservableProperty] private string? _formattedUnsignedInt;
    [ObservableProperty] private string? _formattedSignedLong;
    [ObservableProperty] private string? _formattedUnsignedLong;

    public bool ShowValuesAsHex
    {
        get => _showValuesAsHex;
        set
        {
            if (SetProperty(ref _showValuesAsHex, value))
            {
                RefreshValues();
            }
        }
    }

    public string FormattedCommand => $"0x{Command:X2}";
    public string? FormattedSequence => Sequence is not null ? $"0x{Sequence:X2}" : null;

    public int HexSelectionStart
    {
        get => _hexSelectionStart;
        set
        {
            if (SetProperty(ref _hexSelectionStart, value))
            {
                SyncHexToTextSelection();
                RecalculateBufferIndex();
                RefreshValues();
            }
        }
    }

    public int HexSelectionEnd
    {
        get => _hexSelectionEnd;
        set
        {
            if (SetProperty(ref _hexSelectionEnd, value))
            {
                SyncHexToTextSelection();
                RecalculateBufferIndex();
                RefreshValues();
            }
        }
    }

    public int TextSelectionStart
    {
        get => _textSelectionStart;
        set
        {
            if (SetProperty(ref _textSelectionStart, value))
            {
                SyncTextToHexSelection();
            }
        }
    }

    public int TextSelectionEnd
    {
        get => _textSelectionEnd;
        set
        {
            if (SetProperty(ref _textSelectionEnd, value))
            {
                SyncTextToHexSelection();
            }
        }
    }

    public RawHexViewModel(NetworkPacket packet)
    {
        _payload = packet.Data.ToArray();
        RawHex = string.Join(" ", _payload.Select(b => b.ToString("X2")));
        DecodedText = GetAsciiText();
        
        Sequence = packet switch
        {
            ClientPacket clientPacket => clientPacket.Sequence,
            ServerPacket serverPacket => serverPacket.Sequence,
            _ => null
        };
    }

    private void SyncTextToHexSelection()
    {
        if (_isSyncing)
        {
            return;
        }

        _isSyncing = true;
        
        try
        {
            var textStart = TextSelectionStart;
            var textEnd = TextSelectionEnd;

            if (textStart > textEnd)
            {
                (textStart, textEnd) = (textEnd, textStart);
            }

            var newHexStart = textStart * 3;
            var newHexEnd = textEnd * 3 - 1;

            if (Math.Abs(newHexStart - newHexEnd) < 2)
            {
                HexSelectionStart = 0;
                HexSelectionEnd = 0;
                return;
            }

            HexSelectionStart = newHexStart;
            HexSelectionEnd = newHexEnd;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void SyncHexToTextSelection()
    {
        if (_isSyncing)
        {
            return;
        }

        _isSyncing = true;

        try
        {
            var hexStart = HexSelectionStart;
            var hexEnd = HexSelectionEnd;

            if (hexStart > hexEnd)
            {
                (hexStart, hexEnd) = (hexEnd, hexStart);
            }

            var newTextStart = hexStart / 3;
            var newTextEnd = (hexEnd + 1) / 3;

            TextSelectionStart = newTextStart;
            TextSelectionEnd = newTextEnd;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void RecalculateBufferIndex()
    {
        var startIndex = HexSelectionStart / 3;
        var endIndex = (HexSelectionEnd + 1) / 3;

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
        var selectedSpan = _payload.AsSpan(_startIndex, Math.Abs(_endIndex - _startIndex));
        
        FormattedSignedByte = TryReadFormattedValue<sbyte>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedUnsignedByte = TryReadFormattedValue<byte>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedSignedShort = TryReadFormattedValue<short>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedUnsignedShort = TryReadFormattedValue<ushort>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedSignedInt = TryReadFormattedValue<int>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedUnsignedInt = TryReadFormattedValue<uint>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedSignedLong = TryReadFormattedValue<long>(selectedSpan, ShowValuesAsHex) ?? "--";
        FormattedUnsignedLong = TryReadFormattedValue<ulong>(selectedSpan, ShowValuesAsHex) ?? "--";
    }

    private string GetAsciiText()
    {
        var buffer = ArrayPool<char>.Shared.Rent(_payload.Length + 1);

        try
        {
            var decodedLength = Encoding.ASCII.GetChars(_payload, buffer);
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
        HexSelectionStart = 0;
        HexSelectionEnd = RawHex.Length;
    }

    private static string? TryReadFormattedValue<T>(ReadOnlySpan<byte> buffer, bool isHex = false)
        where T : struct
    {
        var t = typeof(T);
        var numberOfBytes = GetSizeOfType(t);

        if (numberOfBytes > buffer.Length)
        {
            return null;
        }
        
        ulong value = 0;
        for (var i = 0; i < numberOfBytes; i++)
        {
            value = (value << 8) | buffer[i];
        }

        if (isHex)
        {
            return t switch
            {
                _ when t == typeof(bool) || t == typeof(char) => $"0x{value:X2}",
                _ when t == typeof(sbyte) || t == typeof(byte) => $"0x{value:X2}",
                _ when t == typeof(short) || t == typeof(ushort) => $"0x{value:X4}",
                _ when t == typeof(int) || t == typeof(uint) => $"0x{value:X8}",
                _ when t == typeof(long) || t == typeof(ulong) => $"0x{value:X16}",
                _ => $"0x{value:X}" 
            };
        }

        return t switch
        {
            _ when t == typeof(sbyte) => ((sbyte)value).ToString(),
            _ when t == typeof(short) => ((short)value).ToString(),
            _ when t == typeof(int) => ((int)value).ToString(),
            _ when t == typeof(long) => ((long)value).ToString(),
            _ => value.ToString()
        };
    }

    private static int GetSizeOfType(Type type)
    {
        return type switch
        {
            _ when type == typeof(bool) => 1,
            _ when type == typeof(byte) || type == typeof(sbyte) => 1,
            _ when type == typeof(short) || type == typeof(ushort) => 2,
            _ when type == typeof(int) || type == typeof(uint) => 4,
            _ when type == typeof(long) || type == typeof(ulong) => 8,
            _ => throw new NotSupportedException($"Size for type {type.Name} not supported")
        };
    }
}