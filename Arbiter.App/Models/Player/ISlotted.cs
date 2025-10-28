namespace Arbiter.App.Models.Player;

public interface ISlotted
{
    bool IsEmpty { get; }
    int Slot { get; }
    string Name { get; }
}