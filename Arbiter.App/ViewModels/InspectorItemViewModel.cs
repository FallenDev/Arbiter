
namespace Arbiter.App.ViewModels;

public abstract class InspectorItemViewModel : ViewModelBase
{
    private string _name;
    private int _order;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    protected InspectorItemViewModel(string name, int order = int.MaxValue)
    {
        Name = name;
        Order = order;
    }
}