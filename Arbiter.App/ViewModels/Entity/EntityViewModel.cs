using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Flags), nameof(Id), nameof(Name), nameof(Sprite), nameof(X), nameof(Y))]
    [NotifyPropertyChangedFor(nameof(IsPlayer), nameof(IsMonster), nameof(IsMundane), nameof(IsItem),
        nameof(IsReactor))]
    private GameEntity _entity;

    public EntityFlags Flags => Entity.Flags;
    public long Id => Entity.Id;
    public string? Name => Entity.Name;

    public string TypeName => Flags switch
    {
        EntityFlags.Player => "Player",
        EntityFlags.Monster => "Monster",
        EntityFlags.Mundane => "NPC",
        EntityFlags.Item => "Item",
        EntityFlags.Reactor => "Reactor",
        _ => "Unknown"
    };

    public ushort Sprite => Entity.Sprite;
    public int X => Entity.X;
    public int Y => Entity.Y;

    public bool IsPlayer => Flags.HasFlag(EntityFlags.Player);
    public bool IsMonster => Flags.HasFlag(EntityFlags.Monster);
    public bool IsMundane => Flags.HasFlag(EntityFlags.Mundane);
    public bool IsItem => Flags.HasFlag(EntityFlags.Item);
    public bool IsReactor => Flags.HasFlag(EntityFlags.Reactor);

    public EntityViewModel(GameEntity entity)
    {
        Entity = entity;
    }
}