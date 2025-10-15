using Arbiter.App.Models;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityViewModel : ViewModelBase
{
    private readonly GameEntity _entity;

    public long Id => _entity.Id;
    
    public EntityViewModel(GameEntity entity)
    {
        _entity = entity;
    }
}