using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Arbiter.App.Mappings;

public class InspectorTypeOverrideBuilder<T>
{
    private readonly Dictionary<string, InspectorPropertyOverrides> _properties = new(StringComparer.Ordinal);
    private Func<object, string>? _displayNameSelector;
    private Func<object, bool>? _isExpandedPredicate;

    public InspectorTypeOverrideBuilder<T> Property<TProp>(Expression<Func<T, TProp>> expression,
        Action<InspectorTypeOverridePropertyBuilder<T, TProp>>? configure = null)
    {
        var member = GetMemberFromExpression(expression);
        var propertyName = member.Name;
        var builder = new InspectorTypeOverridePropertyBuilder<T, TProp>(member);
        configure?.Invoke(builder);
        _properties[propertyName] = builder.Build();
        return this;
    }

    public InspectorTypeOverrideBuilder<T> DisplayName(Func<T, string> selector)
    {
        _displayNameSelector = o => o is null ? string.Empty : selector((T)o);
        return this;
    }

    public InspectorTypeOverrideBuilder<T> IsExpanded(Func<T, bool> predicate)
    {
        _isExpandedPredicate = o => o is null ? false : predicate((T)o);
        return this;
    }

    internal InspectorTypeOverrideMapping Build()
    {
        return new InspectorTypeOverrideMapping(typeof(T), _properties, _displayNameSelector, _isExpandedPredicate);
    }

    private static MemberInfo GetMemberFromExpression<TProp>(Expression<Func<T, TProp>> expression)
    {
        var body = expression.Body;
        while (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } ue)
        {
            body = ue.Operand;
        }

        return body switch
        {
            MemberExpression me => me.Member,
            MethodCallExpression mce => mce.Method,
            IndexExpression ie => ie.Indexer?.GetMethod ??
                                  throw new ArgumentException("Expression must be a member access"),
            _ => throw new ArgumentException("Expression must be a simple member accessor")
        };
    }
}

public class InspectorTypeOverridePropertyBuilder<T, TProp>
{
    private readonly MemberInfo _member;

    private bool? _showHex;
    private bool? _showMultiline;
    private Func<object, string>? _formatter;
    private bool? _isMasked;
    private char? _maskChar;
    private string? _toolTip;
    private string? _displayName;
    private bool? _isExpanded;

    internal InspectorTypeOverridePropertyBuilder(MemberInfo member)
    {
        _member = member;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> ShowHex(bool showHex = true)
    {
        _showHex = showHex;
        return this;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> ShowMultiline(bool showMultiline = true)
    {
        _showMultiline = showMultiline;
        return this;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> Name(string displayName)
    {
        _displayName = displayName;
        return this;
    }
    
    public InspectorTypeOverridePropertyBuilder<T, TProp> IsExpanded(bool isExpanded)
    {
        _isExpanded = isExpanded;
        return this;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> Format(Func<TProp, string> formatter)
    {
        _formatter = value => value is null ? string.Empty : formatter((TProp)value);
        return this;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> Mask(char maskChar = '●')
    {
        _isMasked = true;
        _maskChar = maskChar;
        return this;
    }

    public InspectorTypeOverridePropertyBuilder<T, TProp> ToolTip(string toolTip)
    {
        _toolTip = toolTip;
        return this;
    }

    internal InspectorPropertyOverrides Build()
    {
        return new InspectorPropertyOverrides(_member.Name, _member)
        {
            ShowHex = _showHex,
            ShowMultiline = _showMultiline,
            Formatter = _formatter,
            IsMasked = _isMasked,
            MaskCharacter = _maskChar,
            ToolTip = _toolTip,
            DisplayName = _displayName,
            IsExpanded = _isExpanded,
        };
    }
}