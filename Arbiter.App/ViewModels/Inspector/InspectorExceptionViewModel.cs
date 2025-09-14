using System;

namespace Arbiter.App.ViewModels.Inspector;

public class InspectorExceptionViewModel : ViewModelBase
{
    public required Exception Exception { get; set; }
}