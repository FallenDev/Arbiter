
namespace Arbiter.App.ViewModels.Client;

public class ClientViewModel : ViewModelBase
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public ClientViewModel()
    {
        
    }
}