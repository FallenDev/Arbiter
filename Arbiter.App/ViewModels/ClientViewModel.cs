
namespace Arbiter.App.ViewModels;

public class ClientViewModel : ViewModelBase
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public ClientViewModel()
    {
        
    }
}