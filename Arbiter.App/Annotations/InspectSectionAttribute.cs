using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InspectSectionAttribute : Attribute
{
    public string Header { get; set; }
    public int Order { get; set; }

    public InspectSectionAttribute(string header, int order = int.MaxValue)
    {
        Header = header;
        Order = order;
    }
}