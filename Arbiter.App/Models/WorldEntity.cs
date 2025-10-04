namespace Arbiter.App.Models;

public record WorldEntity(WorldEntityType Type, uint Id, int X, int Y, int Sprite, string? Name);