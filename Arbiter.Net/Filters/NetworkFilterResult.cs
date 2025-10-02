namespace Arbiter.Net.Filters;

public class NetworkFilterResult
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public NetworkFilterAction Action { get; set; }
    public required NetworkPacket Input { get; set; }
    public NetworkPacket? Output { get; set; }
    public Exception? Exception { get; set; }
}