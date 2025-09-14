using Avalonia.Input;

namespace Arbiter.App.Services;

public interface IKeyboardService
{
    public KeyModifiers GetModifiers();
    public bool IsModifierPressed(KeyModifiers modifiers);
}