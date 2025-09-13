using Arbiter.Net.Client;

namespace Arbiter.App.Mappings.Client;

public class ClientTypeOverridesProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.RegisterOverrides<ClientGroupBox>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline());
            b.Property(m => m.Note, p => p.ShowMultiline());
        });
    }
}