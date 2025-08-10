using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class TraceFilterViewModel : ViewModelBase
{
    [GeneratedRegex(@"[^a-z,\?\*]", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NameFilterRegex();

    public static IReadOnlyList<PacketDirection> AvailablePacketDirections =>
        [PacketDirection.Client, PacketDirection.Server, PacketDirection.Both];

    [ObservableProperty] private PacketDirection _packetDirection = PacketDirection.Both;

    private string _nameFilter = string.Empty;

    [ObservableProperty] private IReadOnlyList<string> _nameFilterPatterns = [];

    public string NameFilter
    {
        get => _nameFilter;
        set
        {
            var sanitized = SanitizeNameFilter(value);
            if (!SetProperty(ref _nameFilter, sanitized))
            {
                return;
            }

            OnPropertyChanged();
            NameFilterPatterns = sanitized.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        }
    }

    private static string SanitizeNameFilter(string value)
        => NameFilterRegex().Replace(value, string.Empty).Trim();
}