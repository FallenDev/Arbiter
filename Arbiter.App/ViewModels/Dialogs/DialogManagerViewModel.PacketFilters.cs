using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogManagerViewModel
{
    private void AddPacketFilters()
    {
        _proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMessage>(OnDialogMessage)
        {
            Name = "DialogManager_ShowDialogMessage",
            Priority = int.MaxValue
        });
        
        _proxyServer.AddFilter(new ServerMessageFilter<ServerShowDialogMenuMessage>(OnDialogMenuMessage)
        {
            Name = "DialogManager_ShowDialogMenuMessage",
            Priority = int.MaxValue
        });
    }

    private NetworkPacket OnDialogMessage(ProxyConnection connection, ServerShowDialogMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMessage> result)
    {
        if (!ShouldSync || !_clientManager.TryGetClient(connection.Id, out var client))
        {
            return result.Passthrough();
        }

        var dialog = BuildDialogView(message);
        SetActiveDialogForClient(client.Id, dialog);

        if (client == SelectedClient)
        {
            ActiveDialog = dialog;
        }

        // Do not alter the packet
        return result.Passthrough();
    }

    private NetworkPacket OnDialogMenuMessage(ProxyConnection connection, ServerShowDialogMenuMessage message,
        object? parameter, NetworkMessageFilterResult<ServerShowDialogMenuMessage> result)
    {
        if (ShouldSync || !_clientManager.TryGetClient(connection.Id, out var client))
        {
            return result.Passthrough();
        }

        var dialog = BuildDialogView(message);
        SetActiveDialogForClient(client.Id, dialog);

        if (client == SelectedClient)
        {
            ActiveDialog = dialog;
        }

        // Do not alter the packet
        return result.Passthrough();
    }

    private void SetActiveDialogForClient(long clientId, DialogViewModel? dialog)
        => _activeDialogs.AddOrUpdate(clientId, dialog, (_, _) => dialog);

    private static DialogViewModel? BuildDialogView(ServerShowDialogMessage message)
    {
        if (message.DialogType == DialogType.CloseDialog)
        {
            return null;
        }
        
        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        var dialog = new DialogViewModel
        {
            Name = name,
            EntityId = message.EntityId,
            Sprite = message.Sprite,
            PursuitId = message.PursuitId,
            StepId = message.StepId,
            Content = message.Content,
            CanNavigatePrevious = message.HasPreviousButton,
            CanNavigateNext = message.HasNextButton,
            CanNavigateTop = message.StepId is > 0,
        };
        
        return dialog;
    }

    private static DialogViewModel? BuildDialogView(ServerShowDialogMenuMessage message)
    {
        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        var dialog = new DialogViewModel
        {
            Name = name,
            EntityId = message.EntityId,
            Sprite = message.Sprite,
            PursuitId = message.PursuitId,
            Content = message.Content
        };

        if (message.MenuChoices.Count > 0)
        {
            foreach (var choice in message.MenuChoices)
            {
                dialog.MenuChoices.Add(new DialogMenuChoiceViewModel
                {
                    Text = choice.Text,
                    PursuitId = choice.PursuitId,
                });
            }
        }

        return dialog;
    }
}