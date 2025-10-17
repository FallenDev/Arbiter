using System;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogManagerViewModel
{
    private void OnDialogMenuChoiceSelected(object? sender, DialogMenuEventArgs e)
    {
        if (SelectedClient is null || ActiveDialog?.EntityId is null || e.SelectedChoice.PursuitId is null)
        {
            return;
        }

        var menuChoiceMessage = new ClientDialogMenuChoiceMessage
        {
            EntityId = (uint)ActiveDialog.EntityId.Value,
            PursuitId = (ushort)e.SelectedChoice.PursuitId
        };
        SelectedClient.EnqueueMessage(menuChoiceMessage);
    }
    
    private void OnDialogNavigatePrevious(object? sender, DialogEventArgs e)
    {
        if (SelectedClient is null || ActiveDialog is null)
        {
            return;
        }

        TryNavigateWithStepOffset(ActiveDialog, -1);
    }

    private void OnDialogNavigateNext(object? sender, DialogEventArgs e)
    {
        if (SelectedClient is null || ActiveDialog is null)
        {
            return;
        }

        TryNavigateWithStepOffset(ActiveDialog, 1);
    }

    private void OnDialogNavigateTop(object? sender, DialogEventArgs e)
    {
        if (SelectedClient is null)
        {
            return;
        }
        
        var entityId = ActiveDialog?.EntityId;
        if (entityId is null or 0)
        {
            return;
        }

        // This just re-interacts with the entity
        var interactMessage = new ClientInteractMessage
        {
            InteractionType = InteractionType.Entity,
            TargetId = (uint)entityId.Value
        };
        
        SelectedClient.EnqueueMessage(interactMessage);
    }

    private void OnDialogClose(object? sender, DialogEventArgs e)
    {
        CloseCurrentDialog();

        if (ShouldSync && SelectedClient is not null)
        {
            SetActiveDialogForClient(SelectedClient.Id, null);
        }

        ActiveDialog = null;
    }

    private void CloseCurrentDialog()
    {
        if (SelectedClient == null)
        {
            return;
        }

        // If there is no step ID, we can just act like the dialog was closed by the server
        if (ActiveDialog?.StepId is null)
        {
            var closeDialogMessage = new ServerShowDialogMessage
            {
                DialogType = DialogType.CloseDialog
            };
            SelectedClient.EnqueueMessage(closeDialogMessage);
            return;
        }

        TryNavigateWithStepOffset(ActiveDialog, 0);
    }

    private bool TryNavigateWithStepOffset(DialogViewModel dialog, int offset)
    {
        if (SelectedClient is null || dialog.EntityId is null or 0 || dialog.PursuitId is null || dialog.StepId is null)
        {
            return false;
        }

        if (dialog.StepId.Value is 0)
        {
            return false;
        }

        // Next/Prev dialogs require an explicit client action to prevent "You are busy" messages
        var closeInteractMessage = new ClientDialogChoiceMessage
        {
            EntityId = (uint)dialog.EntityId,
            EntityType = dialog.EntityType,
            PursuitId = (ushort)dialog.PursuitId,
            StepId = (ushort)Math.Max(0, dialog.StepId.Value + offset)
        };
        return SelectedClient.EnqueueMessage(closeInteractMessage);
    }
}