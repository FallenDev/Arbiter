using System;
using System.Globalization;
using Arbiter.App.Models.Tracing;
using Arbiter.App.ViewModels.Tracing;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Arbiter.App.Converters;

public class PacketDirectionColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xcd, 0xcf, 0xd2));

    private static readonly SolidColorBrush ClientBrush = new(Color.FromRgb(0x82, 0xbf, 0xf7));
    private static readonly SolidColorBrush ClientReplaceBrush = new(Color.FromRgb(0x82, 0xbf, 0xf7));

    private static readonly SolidColorBrush ServerBrush = new(Color.FromRgb(0x83, 0xe6, 0x7a));
    private static readonly SolidColorBrush ServerReplaceBrush = new(Color.FromRgb(0x83, 0xe6, 0x7a));
    
    private static readonly SolidColorBrush BlockedBrush = new(Color.FromRgb(0x71, 0x74, 0x79));
        
    public static PacketDirectionColorConverter Converter => new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var action = NetworkFilterAction.Allow;

        // If value is a TracePacketViewModel, use the filter action and direction from it
        if (value is TracePacketViewModel vm)
        {
            action = vm.FilterAction;

            if (action == NetworkFilterAction.Block)
            {
                return BlockedBrush;
            }

            value = vm.Direction;
        }
        
        return value switch
        {
            PacketDirection => value switch
            {
                PacketDirection.Client => action == NetworkFilterAction.Replace ? ClientReplaceBrush : ClientBrush,
                PacketDirection.Server => action == NetworkFilterAction.Replace ? ServerReplaceBrush : ServerBrush,
                _ => DefaultBrush
            },
            NetworkPacket => value switch
            {
                ClientPacket => action == NetworkFilterAction.Replace ? ClientReplaceBrush : ClientBrush,
                ServerPacket => action == NetworkFilterAction.Replace ? ServerReplaceBrush : ServerBrush,
                _ => DefaultBrush
            },
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}