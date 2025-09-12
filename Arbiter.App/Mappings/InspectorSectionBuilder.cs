using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Arbiter.App.Mappings;

public class InspectorSectionBuilder<T>
{
    private readonly string _header;
    private readonly InspectorTypeBuilder<T> _parent;
    private readonly List<InspectorPropertyBuilder<T>> _properties = [];

    private Func<T, bool>? _isExpandedPredicate;

    public InspectorSectionBuilder(string header, InspectorTypeBuilder<T> parent)
    {
        _header = header;
        _parent = parent;
    }

    public InspectorSectionBuilder<T> IsExpanded(Func<T, bool> predicate)
    {
        _isExpandedPredicate = predicate;
        return this;
    }

    public InspectorSectionBuilder<T> Property<TProp>(Expression<Func<T, TProp>> expression,
        Action<InspectorPropertyBuilder<T, TProp>>? configure = null)
    {
        var builder = new InspectorPropertyBuilder<T, TProp>(expression);
        configure?.Invoke(builder);

        _properties.Add(builder.Untyped);
        return this;
    }

    public InspectorSectionMapping Build()
    {
        Func<object, bool>? isExpanded = null;
        if (_isExpandedPredicate is not null)
        {
            isExpanded = instance => _isExpandedPredicate.Invoke((T)instance);
        }

        var props = _properties.Select(p => p.Build());
        return new InspectorSectionMapping(_header, props, isExpanded);
    }
}