namespace Arbiter.App.ViewModels.Dialogs;

public class DialogMenuEventArgs : DialogEventArgs
{
    public DialogMenuChoiceViewModel SelectedChoice { get; }

    public DialogMenuEventArgs(DialogMenuChoiceViewModel choice)
    {
        SelectedChoice = choice;
    }
}