using Arbiter.Net.Client.Types;

namespace Arbiter.App.Mappings.Client;

public class ClientTypeOverridesProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.RegisterOverrides<ClientGroupBox>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display text for the floating group box."))
                .Property(m => m.Note, p => p.ShowMultiline().ToolTip("Description for the group."))
                .Property(m => m.MinLevel, p => p.ToolTip("Minimum level for group members."))
                .Property(m => m.MaxLevel, p => p.ToolTip("Maximum level for group members."))
                .Property(m => m.MaxWarriors, p => p.ToolTip("Number of desired warriors for the group"))
                .Property(m => m.MaxWizards, p => p.ToolTip("Number of desired wizards for the group."))
                .Property(m => m.MaxRogues, p => p.ToolTip("Number of desired rogues for the group."))
                .Property(m => m.MaxPriests, p => p.ToolTip("Number of desired priests for the group."))
                .Property(m => m.MaxMonks, p => p.ToolTip("Number of desired monks for the group."));
        });
    }
}