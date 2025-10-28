using Avalonia.Input;

namespace Arbiter.App.Services.Input;

public interface IKeyboardService
{
    public KeyModifiers GetModifiers();
    public bool IsModifierPressed(KeyModifiers modifiers);
}