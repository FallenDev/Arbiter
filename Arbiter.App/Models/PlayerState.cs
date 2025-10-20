namespace Arbiter.App.Models;

public sealed class PlayerState
{
    public int ConnectionId { get; }
    public long? UserId { get; set; }
    
    public string? Name { get; set; }
    
    public string? Class { get; set; }
    public int Level { get; set; }
    public int AbilityLevel { get; set; }
    public string? MapName { get; set; }
    public int? MapId { get; set; }
    public int? MapX { get; set; }
    public int? MapY { get; set; }
    
    public long CurrentHealth { get; set; }
    public long MaxHealth { get; set; }
    public long CurrentMana { get; set; }
    public long MaxMana { get; set; }
    
    public PlayerInventory Inventory { get; set; } = new();

    public PlayerState(int connectionId, string? name)
    {
        ConnectionId = connectionId;
        Name = name;
    }
}