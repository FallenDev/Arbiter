
namespace Arbiter.App.ViewModels.Client;

public partial class ClientManagerViewModel
{
    private void AddSuperLookToClients()
    {
        foreach (var client in _clients.Values)
        {
            AddSuperLookSkill(client);
        }
    }

    private void RemoveSuperLookFromClients()
    {
        foreach (var client in _clients.Values)
        {
            client.RemoveVirtualSkill("Super Look");
        }
    }

    private void AddSuperLookSkill(ClientViewModel client)
    {
        if (client.Player.Skillbook.HasSkill("SuperLook"))
        {
            return;
        }

        client.AddVirtualSkill(74, 1, "Super Look", () => PerformSuperLook(client));
    }

    private void PerformSuperLook(ClientViewModel client)
    {
        
    }
}