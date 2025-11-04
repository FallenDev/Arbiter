
namespace Arbiter.App.ViewModels.Client;

public partial class ClientManagerViewModel
{
    private void AddSuperLookToClients()
    {
        foreach (var client in _clients.Values)
        {
            if (client.Player.Skillbook.HasSkill("SuperLook"))
            {
                continue;
            }
            
            client.AddVirtualSkill(74, 1, "Super Look", PerformSuperLook);
        }
    }

    private void RemoveSuperLookFromClients()
    {
        foreach (var client in _clients.Values)
        {
            client.RemoveVirtualSkill("Super Look");
        }
    }

    private void PerformSuperLook()
    {
        
    }
}