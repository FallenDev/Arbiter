using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.MessageBox;

// This is only used at design-time to make it easier to adjust XAML view layout.
public class DesignMessageBoxViewModel : MessageBoxViewModel
{
    public DesignMessageBoxViewModel()
    {
        Title = "Message Box";
        Message = "This is a message box that can display a message.\nIt can even span multiple lines.";
        Description = "This is a description of the message box.";
        Style = MessageBoxStyle.YesNo;
        AcceptButtonText = "OK";
        CancelButtonText = "Cancel";
    }
}