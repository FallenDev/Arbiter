using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InspectToolTipAttribute : Attribute
{
    public string? ToolTip { get; set; }

    public InspectToolTipAttribute(string toolTipText)
    {
        ToolTip = toolTipText;
    }
}