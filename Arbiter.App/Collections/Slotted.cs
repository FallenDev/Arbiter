
namespace Arbiter.App.Collections;

public readonly struct Slotted<T> where T : class
{
    public int Slot { get; init; }
    
    public T Value { get; init; }

    public Slotted(int slot, T value)
    {
        Slot = slot;
        Value = value;
    }
}