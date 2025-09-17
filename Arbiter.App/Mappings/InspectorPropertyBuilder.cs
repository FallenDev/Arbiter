using System;
using System.Linq.Expressions;
using System.Reflection;
using Arbiter.App.Extensions;

namespace Arbiter.App.Mappings;

public class InspectorPropertyBuilder<T>
{
    private readonly MemberInfo? _member;
    private readonly Type _propertyType;
    private readonly Func<object, object?> _getter;

    private bool _showHex;
    private bool _showMultiline;
    private string? _name;
    private Func<object, string>? _formatter;
    private bool _isMasked;
    private char? _maskChar;
    private string? _toolTip;

    internal InspectorPropertyBuilder(MemberInfo? member, Type propertyType, Func<object, object?> getter)
    {
        _member = member;
        _propertyType = propertyType;
        _getter = getter;
    }

    public InspectorPropertyBuilder<T> ShowHex(bool showHex = true)
    {
        _showHex = showHex;
        return this;
    }

    public InspectorPropertyBuilder<T> ShowMultiline(bool showMultiline = true)
    {
        _showMultiline = showMultiline;
        return this;
    }

    public InspectorPropertyBuilder<T> Name(string name)
    {
        _name = name;
        return this;
    }

    public InspectorPropertyBuilder<T> Format(Func<object, string> formatter)
    {
        _formatter = formatter;
        return this;
    }

    public InspectorPropertyBuilder<T> Mask(char maskChar = '*')
    {
        _isMasked = true;
        _maskChar = maskChar;
        return this;
    }

    public InspectorPropertyBuilder<T> ToolTip(string toolTip)
    {
        _toolTip = toolTip;
        return this;
    }
    
    public InspectorPropertyMapping Build()
    {
        var displayName = _name ?? (_member?.Name.ToNaturalWording() ?? string.Empty);
        return new InspectorPropertyMapping(displayName, _member, _propertyType, _getter)
        {
            ShowHex = _showHex,
            ShowMultiline = _showMultiline,
            Formatter = _formatter,
            IsMasked = _isMasked,
            MaskCharacter = _maskChar,
            ToolTip = _toolTip,
        };
    }
}

public class InspectorPropertyBuilder<T, TProp>
{
    internal InspectorPropertyBuilder<T> Untyped { get; }

    internal InspectorPropertyBuilder(Expression<Func<T, TProp>> expression)
    {
        var member = GetMemberFromExpression(expression);
        var getter = CreateGetter(expression);

        Untyped = new InspectorPropertyBuilder<T>(member, typeof(TProp), getter);
    }

    // Constructor for computed properties (no backing MemberInfo)
    internal InspectorPropertyBuilder(string name, Func<T, TProp> getter)
    {
        Func<object, object?> wrapped = obj =>
        {
            if (obj is null)
            {
                return default(TProp);
            }
            return getter((T)obj);
        };
        Untyped = new InspectorPropertyBuilder<T>(null, typeof(TProp), wrapped);
        Untyped.Name(name);
    }

    private static MemberInfo GetMemberFromExpression(Expression<Func<T, TProp>> expression)
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

    private static Func<object, object?> CreateGetter(Expression<Func<T, TProp>> expression)
    {
        var compiled = expression.Compile();
        return obj =>
        {
            if (obj is null)
            {
                return default(TProp);
            }

            return compiled((T)obj);
        };
    }

    public InspectorPropertyBuilder<T, TProp> ShowHex(bool showHex = true)
    {
        Untyped.ShowHex(showHex);
        return this;
    }

    public InspectorPropertyBuilder<T, TProp> ShowMultiline(bool showMultiline = true)
    {
        Untyped.ShowMultiline(showMultiline);
        return this;
    }

    public InspectorPropertyBuilder<T, TProp> Name(string name)
    {
        Untyped.Name(name);
        return this;
    }

    public InspectorPropertyBuilder<T, TProp> Format(Func<TProp, string> formatter)
    {
        Untyped.Format(WrappedFormatter);
        return this;

        string WrappedFormatter(object value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return formatter((TProp)value);
        }
    }
    
    public InspectorPropertyBuilder<T, TProp> Mask(char maskChar = '●')
    {
        Untyped.Mask(maskChar);
        return this;
    }

    public InspectorPropertyBuilder<T, TProp> ToolTip(string toolTip)
    {
        Untyped.ToolTip(toolTip);
        return this;
    }
}