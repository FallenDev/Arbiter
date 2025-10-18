namespace Arbiter.App.Models;

public sealed class PlayerState
{
    public int ConnectionId { get; }
    public string? Name { get; set; }

    public long? UserId { get; set; }
    public string? Class { get; set; }

    public string? MapName { get; set; }
    public int? MapId { get; set; }
    public int? MapX { get; set; }
    public int? MapY { get; set; }


    public PlayerState(int connectionId, string? name)
    {
        ConnectionId = connectionId;
        Name = name;
    }
}