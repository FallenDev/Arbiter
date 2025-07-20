namespace Arbiter.App.Models;

public enum MessageBoxStyle
{
    Ok = 0,
    YesNo = 1,
}

public class MessageBoxDetails
{
    public string Title { get; set; } = "Message Box";
    public string Message { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MessageBoxStyle Style { get; set; } = MessageBoxStyle.Ok;
    public string AcceptButtonText { get; set; } = "OK";
    public string CancelButtonText { get; set; } = "Cancel";
}