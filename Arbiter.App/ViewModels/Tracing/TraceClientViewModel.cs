namespace Arbiter.App.ViewModels.Tracing;

public class TraceClientViewModel
{
    public string DisplayName { get; set; }
    public string? Name { get; set; }

    public TraceClientViewModel(string displayName, string? name = null)
    {
        DisplayName = displayName;
        Name = name;
    }
}