using Arbiter.App.Extensions;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerTypeOverridesProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.RegisterOverrides<ServerMessageBoardInfo>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline())
                .DisplayName(m => m.Name);
        });

        registry.RegisterOverrides<ServerMessageBoardPost>(b =>
        {
            b.Property(m => m.Subject, p => p.ShowMultiline())
                .DisplayName(m => $"Post {m.Id}");
        });

        registry.RegisterOverrides<ServerCreatureEntity>(b =>
        {
            b.Property(m => m.Id, p => p.ShowHex())
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Unknown, p => p.ShowHex());
        });

        registry.RegisterOverrides<ServerDialogMenuChoice>(b => { b.Property(m => m.Text, p => p.ShowMultiline()); });

        registry.RegisterOverrides<ServerEquipmentInfo>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex())
                .DisplayName(m => m.Slot.ToString().ToNaturalWording());
        });

        registry.RegisterOverrides<ServerGroupBox>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline())
                .Property(m => m.Note, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerItemEntity>(b =>
        {
            b.Property(m => m.Id, p => p.ShowHex())
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Unknown, p => p.ShowHex());
        });

        registry.RegisterOverrides<ServerItemMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Name, p => p.ShowMultiline())
                .Property(m => m.Description, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerLegendMark>(b => { b.Property(m => m.Text, p => p.ShowMultiline()); });

        registry.RegisterOverrides<ServerMetadataEntry>(b => { b.Property(m => m.Checksum, p => p.ShowHex()); });

        registry.RegisterOverrides<ServerSpellMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Name, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerSkillMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Name, p => p.ShowMultiline());
        });

        registry.RegisterOverrides<ServerWorldListUser>(b =>
        {
            b.Property(m => m.Flags, p => p.ShowHex())
                .DisplayName(m => m.Name);
        });

        registry.RegisterOverrides<ServerWorldMapNode>(b =>
        {
            b.Property(m => m.Name, p => p.ShowMultiline())
                .Property(m => m.Checksum, p => p.ShowHex());
        });
    }
}