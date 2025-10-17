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
            EntityType = message.EntityType,
            Sprite = message.Sprite,
            PursuitId = message.PursuitId,
            StepId = message.StepId,
            Content = message.Content,
            CanNavigatePrevious = message.HasPreviousButton,
            CanNavigateNext = message.HasNextButton
        };

        if (message.DialogType == DialogType.Menu)
        {
            for (var i = 0; i < message.MenuChoices.Count; i++)
            {
                dialog.MenuChoices.Add(new DialogMenuChoiceViewModel
                {
                    Index = i + 1,
                    Text = message.MenuChoices[i]
                });
            }
        }
        else if (message.DialogType == DialogType.TextInput)
        {
            dialog.IsTextInput = true;
            dialog.PromptLine1 = message.InputPrompt;
            dialog.PromptLine2 = message.InputDescription;
            dialog.InputMaxLength = message.InputMaxLength ?? 255;
            dialog.CanNavigateNext = false;
        }

        return dialog;
    }

    private static DialogViewModel BuildDialogView(ServerShowDialogMenuMessage message)
    {
        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        var dialog = new DialogViewModel
        {
            Name = name,
            EntityId = message.EntityId,
            EntityType = message.EntityType,
            Sprite = message.Sprite,
            PursuitId = message.PursuitId,
            Content = message.Content,
            CanNavigateTop = true
        };

        if (message.MenuChoices.Count > 0)
        {
            for (var i = 0; i < message.MenuChoices.Count; i++)
            {
                var choice = message.MenuChoices[i];
                dialog.MenuChoices.Add(new DialogMenuChoiceViewModel
                {
                    Index = i + 1,
                    Text = choice.Text,
                    PursuitId = choice.PursuitId,
                });
            }
        }

        return dialog;
    }
}