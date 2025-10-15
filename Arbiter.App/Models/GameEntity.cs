namespace Arbiter.App.Models;

public readonly record struct GameEntity
{
    public long Id { get; init; }
    public EntityFlags Flags { get; init; }
    public ushort Sprite { get; init; }
    public string? Name { get; init; }

    // Location where the entity was last seen
    public int? MapId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}