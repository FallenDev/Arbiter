using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceFilterViewModel : ViewModelBase
{
    [GeneratedRegex(@"^([a-z,\?\*]{1,13},?)+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NameFilterRegex();

    public static IReadOnlyList<PacketDirection> AvailablePacketDirections =>
        [PacketDirection.Client, PacketDirection.Server, PacketDirection.Both];

    [ObservableProperty] private PacketDirection _packetDirection = PacketDirection.Both;

    private string _nameFilter = string.Empty;
    private string _commandFilter = string.Empty;

    [ObservableProperty] private IReadOnlyList<string> _nameFilterPatterns = [];
    [ObservableProperty] private IReadOnlyList<ValueRange<byte>> _commandFilterRanges = [];

    public string NameFilter
    {
        get => _nameFilter;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && !NameFilterRegex().IsMatch(value))
            {
                throw new ValidationException("Invalid name filter");
            }

            if (!SetProperty(ref _nameFilter, value))
            {
                return;
            }

            NameFilterPatterns = value.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        }
    }

    public string CommandFilter
    {
        get => _commandFilter;
        set
        {
            if (!SetProperty(ref _commandFilter, value))
            {
                return;
            }

            var parsedRanges = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(range => ValueRange<byte>.ParseByteRange(range.TrimEnd('-'), NumberStyles.HexNumber));

            try
            {
                var ranges = parsedRanges.ToList();
                if (ranges.Any(range => range.Min > range.Max))
                {
                    throw new ValidationException("Invalid command filter range");
                }

                CommandFilterRanges = ranges;
            }
            catch
            {
                throw new ValidationException("Invalid command filter range");
            }
        }
    }

    [RelayCommand]
    private void ClearNameFilter()
    {
        NameFilter = string.Empty;
    }

    [RelayCommand]
    private void ClearCommandFilter()
    {
        CommandFilter = string.Empty;
    }
}