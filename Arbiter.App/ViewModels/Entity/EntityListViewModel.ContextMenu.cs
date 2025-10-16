using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Avalonia;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityListViewModel
{
    private bool CanCopyToClipboard() => SelectedEntities.Count == 1;
    
    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyIdToClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null || SelectedEntities.Count == 0)
        {
            return;
        }

        await clipboard.SetTextAsync(SelectedEntities[0].Id.ToString("X"));
    }
    
    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyHexToClipboard()
    {
        if (SelectedEntities.Count == 0)
        {
            return;
        }
        
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null || SelectedEntities.Count == 0)
        {
            return;
        }

        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)SelectedEntities[0].Id);
        await clipboard.SetTextAsync(buffer.ToHexString(" "));
    }
    
    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopySpriteToClipboard()
    {
        if (SelectedEntities.Count == 0)
        {
            return;
        }
        
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null || SelectedEntities.Count == 0)
        {
            return;
        }

        await clipboard.SetTextAsync(SelectedEntities[0].Sprite.ToString());
    }
    
    private bool CanDeleteSelected() => SelectedEntities.Count > 0;

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private void DeleteSelected()
    {
        if (SelectedEntities.Count == 0)
        {
            return;
        }
        
        var entities = SelectedEntities.ToList();
        foreach (var entity in entities)
        {
            _entityStore.RemoveEntity(entity.Id, out _);
            _allEntities.Remove(entity);
        }

        SelectedEntities.Clear();
    }
}