using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Mappings;

public class InspectorTypeBuilder<T>
{
    private readonly List<InspectorSectionBuilder<T>> _sections = [];

    private string _displayName;

    public InspectorTypeBuilder()
    {
        _displayName = typeof(T).Name;
    }

    public InspectorTypeBuilder<T> Name(string displayName)
    {
        _displayName = displayName;
        return this;
    }
    
    public InspectorSectionBuilder<T> Section(string header)
    {
        var builder = new InspectorSectionBuilder<T>(header, this);
        _sections.Add(builder);
        return builder;
    }

    public InspectorTypeMapping Build()
    {
        var sections = _sections.Select(s => s.Build());
        return new InspectorTypeMapping(typeof(T), _displayName, sections);
    }
}