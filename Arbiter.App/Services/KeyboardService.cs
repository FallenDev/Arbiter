using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arbiter.App.Services;

public class KeyboardService : IKeyboardService
{
    private KeyModifiers _modifiers = KeyModifiers.None;
    
    public KeyboardService()
    {
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, RoutingStrategies.Tunnel, true);
        InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, RoutingStrategies.Tunnel, true);
    }

    public KeyModifiers GetModifiers() => _modifiers;
    public bool IsModifierPressed(KeyModifiers modifiers) => (_modifiers & modifiers) == modifiers;
    
    private void OnKeyDown(TopLevel sender, KeyEventArgs e)
    {
        _modifiers |= e.KeyModifiers;
    }

    private void OnKeyUp(TopLevel sender, KeyEventArgs e)
    {
        _modifiers &= ~e.KeyModifiers;
    }
}