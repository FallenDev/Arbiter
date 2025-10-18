﻿using System;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private NetworkFilterRef? _debugDialogFilter;
    private NetworkFilterRef? _debugDialogMenuFilter;

    private void AddDebugDialogFilters(DebugSettings settings)
    {
        _debugDialogFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerShowDialogMessage>(HandleDialogMessage, settings)
            {
                Name = $"{FilterPrefix}_Dialog_ServerShowDialog",
                Priority = DebugFilterPriority
            });


        _debugDialogMenuFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerShowDialogMenuMessage>(HandleDialogMenuMessage, settings)
            {
                Name = $"{FilterPrefix}_Dialog_ServerShowDialogMenu",
                Priority = DebugFilterPriority
            });
    }

    private void RemoveDebugDialogFilters()
    {
        _debugDialogFilter?.Unregister();
        _debugDialogMenuFilter?.Unregister();
    }

    private static NetworkPacket HandleDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        if (parameter is not DebugSettings settings || settings is { ShowDialogId: false, ShowPursuitId: false })
        {
            return result.Passthrough();
        }

        var hasChanges = false;

        if (settings.ShowDialogId)
        {
            var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
            message.Name = $"{name} {{=h[0x{message.EntityId:X4}]";
            hasChanges = true;
        }

        if (settings.ShowPursuitId)
        {
            var pursuitText = $"{{=ePursuit {message.PursuitId} => Step {message.StepId}";
            message.Name = $"{message.Name} {pursuitText}";
            hasChanges = true;

        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }

    private static NetworkPacket HandleDialogMenuMessage(ProxyConnection connection,
        ServerShowDialogMenuMessage message, object? parameter,
        NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        if (parameter is not DebugSettings settings || settings is { ShowDialogId: false, ShowPursuitId: false })
        {
            return result.Passthrough();
        }

        var hasChanges = false;

        if (settings.ShowDialogId)
        {
            var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
            message.Name = $"{name} {{=h[0x{message.EntityId:X4}]";
            hasChanges = true;
        }

        if (settings.ShowPursuitId)
        {
            if (message.PursuitId is > 0)
            {
                var pursuitText = $"{{=ePursuit {message.PursuitId}";
                message.Name = $"{message.Name} - {pursuitText}";
                hasChanges = true;
            }

            if (message.MenuChoices.Count > 0)
            {
                foreach (var choice in message.MenuChoices)
                {
                    var menuPursuitText = $"{{=j[{choice.PursuitId}]";
                    var maxChoiceLength = 50 - menuPursuitText.Length;
                    var choiceText = choice.Text.Length > maxChoiceLength
                        ? string.Concat(choice.Text.AsSpan(0, maxChoiceLength - 3), "...")
                        : choice.Text;
                    ;

                    choice.Text = $"{choiceText} {menuPursuitText}";
                }

                hasChanges = true;
            }
        }

        return hasChanges ? result.Replace(message) : result.Passthrough();
    }
}