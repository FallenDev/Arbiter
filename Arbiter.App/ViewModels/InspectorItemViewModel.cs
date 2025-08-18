
namespace Arbiter.App.ViewModels;

public abstract class InspectorItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private int _order = int.MaxValue;

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
}