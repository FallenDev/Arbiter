namespace Arbiter.App.ViewModels.Dialogs;

// This is only used at design-time to make it easier to adjust XAML view layout.
internal class DesignDialogViewModel : DialogViewModel
{
    public DesignDialogViewModel()
    {
        Name = "Dialog";
        Content =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        Sprite = 1024;
        EntityId = 0x12345678;
        PursuitId = 99;
        StepId = 33;
        
        AddChoices();
    }

    private void AddChoices()
    {
        for (var i = 0; i < 10; i++)
        {
            var vm = new DialogMenuChoiceViewModel
            {
                PursuitId = (i + 1) * 10,
                Text = $"Menu Choice {i + 1}"
            };

            MenuChoices.Add(vm);
        }
    }
}