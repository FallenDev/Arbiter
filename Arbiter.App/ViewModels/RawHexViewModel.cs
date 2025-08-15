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
    
    private bool _showValuesAsHex;
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
                RefreshValues(false);
            }
        }
    }
    
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

    private void RefreshValues(bool includeText = true)
    {
        if (includeText)
        {
            DecodedText = GetDecodedText();
        }

        FormattedSignedByte = TryReadFormattedValue<sbyte>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedUnsignedByte = TryReadFormattedValue<byte>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedSignedShort = TryReadFormattedValue<short>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedUnsignedShort = TryReadFormattedValue<ushort>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedSignedInt = TryReadFormattedValue<int>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedUnsignedInt = TryReadFormattedValue<uint>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedSignedLong = TryReadFormattedValue<long>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
        FormattedUnsignedLong = TryReadFormattedValue<ulong>(_payload, _startIndex, ShowValuesAsHex) ?? "--";
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

    private static string? TryReadFormattedValue<T>(ReadOnlySpan<byte> buffer, int startIndex, bool isHex = false)
        where T : struct
    {
        if (startIndex < 0 || startIndex >= buffer.Length)
        {
            return null;
        }

        var t = typeof(T);
        var numberOfBytes = GetSizeOfType(t);

        if (startIndex + numberOfBytes > buffer.Length)
        {
            return null;
        }

        ulong value = 0;
        for (var i = 0; i < numberOfBytes; i++)
        {
            value |= (ulong)buffer[startIndex + i] << (i * 8);
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