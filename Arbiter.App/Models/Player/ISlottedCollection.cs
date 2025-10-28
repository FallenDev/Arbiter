using System.Collections.Generic;

namespace Arbiter.App.Models.Player;

public interface ISlottedCollection<T> : IReadOnlyCollection<T> where T : ISlotted
{

}