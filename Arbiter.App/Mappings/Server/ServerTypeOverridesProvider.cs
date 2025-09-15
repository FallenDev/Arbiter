using Arbiter.App.Extensions;
using Arbiter.Net.Server;

namespace Arbiter.App.Mappings.Server;

public class ServerTypeOverridesProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.RegisterOverrides<ServerCreatureEntity>(b =>
        {
            b.Property(m => m.Id, p => p.ShowHex());
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.Property(m => m.Unknown, p => p.ShowHex());
        });
        
        registry.RegisterOverrides<ServerDialogMenuChoice>(b =>
        {
            b.Property(m => m.Text, p => p.ShowMultiline());
        });
        
        registry.RegisterOverrides<ServerEquipmentInfo>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.DisplayName(m => m.Slot.ToString().ToNaturalWording());
        });

        registry.RegisterOverrides<ServerGroupBox>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline());
            b.Property(m => m.Note, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerItemEntity>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.Property(m => m.Unknown, p => p.ShowHex());
        });
        
        registry.RegisterOverrides<ServerItemMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.Property(m => m.Name, p => p.ShowMultiline());
            b.Property(m => m.Description, p => p.ShowMultiline());
        });
        
        registry.RegisterOverrides<ServerLegendMark>(b =>
        {
            b.Property(m => m.Text, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerMetadataEntry>(b =>
        {
            b.Property(m => m.Checksum, p => p.ShowHex());
        });

        registry.RegisterOverrides<ServerSpellMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.Property(m => m.Name, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerSkillMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex());
            b.Property(m => m.Name, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerWorldMapNode>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline());
            b.Property(m => m.Checksum, p => p.ShowHex());
        });
    }
}