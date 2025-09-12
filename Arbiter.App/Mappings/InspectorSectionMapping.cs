using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Mappings;

public class InspectorSectionMapping
{
    public string Header { get; }
    public Func<object, bool> IsExpanded { get; }
    public IReadOnlyList<InspectorPropertyMapping> Properties { get; }

    public InspectorSectionMapping(string header, IEnumerable<InspectorPropertyMapping> properties,
        Func<object, bool>? isExpanded = null)
    {
        Header = header;
        Properties = properties.ToList();
        IsExpanded = isExpanded ?? (_ => true);
    }
}