
namespace Arbiter.App.ViewModels.Dialogs;

public partial class DialogManagerViewModel
{
    private void OnDialogMenuChoiceSelected(object? sender, DialogMenuEventArgs e)
    {
        
    }
    
    private void OnDialogNavigatePrevious(object? sender, DialogEventArgs e)
    {

    }

    private void OnDialogNavigateNext(object? sender, DialogEventArgs e)
    {

    }

    private void OnDialogNavigateTop(object? sender, DialogEventArgs e)
    {

    }

    private void OnDialogClose(object? sender, DialogEventArgs e)
    {
        ActiveDialog = null;

        if (SelectedClient is not null)
        {
            SetActiveDialogForClient(SelectedClient.Id, null);
        }
    }
}