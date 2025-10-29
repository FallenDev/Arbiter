namespace Arbiter.App.Models.Settings;

public class MessageFilter
{
    public bool IsEnabled { get; set; } = true;

    public required string Pattern { get; set; }
}