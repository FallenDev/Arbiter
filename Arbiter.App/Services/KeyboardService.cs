using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arbiter.App.Services;

public class KeyboardService : IKeyboardService
{
    private KeyModifiers _modifiers = KeyModifiers.None;
    
    public KeyboardService()
    {
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, RoutingStrategies.Bubble, true);
        InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, RoutingStrategies.Bubble, true);
    }

    public KeyModifiers GetModifiers() => _modifiers;
    public bool IsModifierPressed(KeyModifiers modifiers) => (_modifiers & modifiers) != 0;
    
    private void OnKeyDown(TopLevel sender, KeyEventArgs e)
    {
        _modifiers |= ModifiersFromKey(e.Key);
    }

    private void OnKeyUp(TopLevel sender, KeyEventArgs e)
    {
        _modifiers &= ~ModifiersFromKey(e.Key);
    }

    private static KeyModifiers ModifiersFromKey(Key key) => key switch
    {
        Key.LeftShift or Key.RightShift => KeyModifiers.Shift,
        Key.LeftCtrl or Key.RightCtrl => KeyModifiers.Control,
        Key.LeftAlt or Key.RightAlt => KeyModifiers.Alt,
        Key.LWin or Key.RWin => KeyModifiers.Meta,
        _ => KeyModifiers.None
    };
}