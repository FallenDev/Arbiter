using System.Collections.Generic;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugShowEffectFilterName = "Debug_ShowEffectFilter";
    private const string DebugUpdateStatsFilterName = "Debug_UpdateStatsFilter";

    private static readonly Dictionary<int, (ushort Effect, ushort? DurationOverride)> ClassicEffectReplacements = new()
    {
        [279] = (2, null), // modern ao puin => classic ao puin
        [232] = (2, null), // modern ao dall/suain => classic ao dall/suain
        [267] = (4, null), // modern ioc => classic ioc
        [244] = (6, null), // modern dion => classic dion
        [271] = (6, null), // modern deireas faileas => classic deireas faileas
        [245] = (8, null), // modern ao curse => classic ao curse
        [234] = (9, null), // modern sal => classic sal
        [235] = (10, null), // modern mor sal => classic mor sal
        [236] = (11, null), // modern ard sal => classic ard sal
        [237] = (12, null), // modern srad => classic srad
        [238] = (13, null), // modern mor srad => classic mor srad
        [239] = (14, null), // modern ard srad => classic ard srad
        [240] = (15, null), // modern athar => classic athar
        [241] = (16, null), // modern mor athar => classic mor athar
        [242] = (17, null), // modern ard athar => classic ard athar
        [243] = (18, null), // modern mor cradh => classic mor cradh
        [280] = (19, null), // modern beannaich => classic beannaich
        [231] = (21, null), // modern aite => classic aite
        [273] = (23, null), // modern deo saighead / fas nadur => classic deo saighead / fas nadur
        [247] = (25, null), // modern poison bubble => classic pink swirl
        [248] = (26, null), // modern windblade => classic windblade
        [283] = (26, null), // modern stab and twist => classic stab and twist
        [274] = (27, null), // modern kick / rescue => classic kick / rescue
        [250] = (29, null), // modern creag => classic creag
        [251] = (30, null), // modern mor creag => classic mor creag
        [252] = (31, null), // modern ard creag => classic ard creag
        [257] = (43, null), // modern ard cradh => classic ard cradh
        [258] = (44, null), // modern cradh => classic cradh
        [259] = (45, null), // modern beag creag => classic beag creag
        [254] = (52, null), // modern mor strioch pian gar => classic mor strioch pian gar
        [380] = (71, null), // modern nuadhaich => classic nuadhaich
        [263] = (81, null), // modern pian na dion => classic pian na dion
        [278] = (195, null), // modern mor pian na dion => classic mor pian na dion
    };

    private void AddDebugEffectsFilters(DebugSettings settings)
    {
        if (settings.UseClassicEffects)
        {
            _proxyServer.AddFilter(ServerCommand.ShowEffect,
                new NetworkPacketFilter(HandleShowEffectMessage, settings)
                {
                    Name = DebugShowEffectFilterName,
                    Priority = DebugFilterPriority
                });
        }

        if (settings.DisableBlind)
        {
            _proxyServer.AddFilter(ServerCommand.UpdateStats,
                new NetworkPacketFilter(HandleUpdateStatsMessage, settings)
                {
                    Name = DebugUpdateStatsFilterName,
                    Priority = DebugFilterPriority
                });
        }
    }

    private void RemoveDebugEffectsFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.ShowEffect, DebugShowEffectFilterName);
        _proxyServer.RemoveFilter(ServerCommand.UpdateStats, DebugUpdateStatsFilterName);
    }

    private NetworkPacket HandleShowEffectMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { UseClassicEffects: false } ||
            !_serverMessageFactory.TryCreate<ServerShowEffectMessage>(serverPacket, out var message))
        {
            return packet;
        }

        var hasReplacement = false;

        // Replace the target animation if it exists
        if (ClassicEffectReplacements.TryGetValue(message.TargetAnimation, out var targetReplacement))
        {
            var (newEffect, animationDurationOverride) = targetReplacement;
            message.TargetAnimation = newEffect;
            message.AnimationDuration = animationDurationOverride ?? message.AnimationDuration;

            hasReplacement = true;
        }

        // Replace the source animation if it exists
        if (message.SourceAnimation.HasValue &&
            ClassicEffectReplacements.TryGetValue(message.SourceAnimation.Value, out var sourceReplacement))
        {
            var (newEffect, animationDurationOverride) = sourceReplacement;
            message.SourceAnimation = newEffect;
            message.AnimationDuration = animationDurationOverride ?? message.AnimationDuration;

            hasReplacement = true;
        }

        if (!hasReplacement)
        {
            return packet;
        }

        // Build a new packet with the modified effect data
        var builder = new NetworkPacketBuilder(ServerCommand.ShowEffect);
        message.Serialize(builder);

        return builder.ToPacket();
    }

    private NetworkPacket HandleUpdateStatsMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { DisableBlind: false } ||
            !_serverMessageFactory.TryCreate<ServerUpdateStatsMessage>(serverPacket, out var message))
        {
            return packet;
        }

        message.IsBlinded = false;

        // Build a new packet with the modified stats data
        var builder = new NetworkPacketBuilder(ServerCommand.UpdateStats);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}