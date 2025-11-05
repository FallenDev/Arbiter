namespace Arbiter.App.Collections;

public readonly struct Slotted<T>
{
    public int Slot { get; init; }
    public T? Value { get; init; }

    public bool HasValue => Value is not null;

    public Slotted(int slot, T? value)
    {
        Slot = slot;
        Value = value;
    }
}