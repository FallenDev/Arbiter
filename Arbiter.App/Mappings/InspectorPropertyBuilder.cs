using System;
using System.Linq.Expressions;
using System.Reflection;
using Arbiter.App.Extensions;

namespace Arbiter.App.Mappings;

public class InspectorPropertyBuilder<T>
{
    private readonly MemberInfo _member;
    private readonly Type _propertyType;
    private readonly Func<object, object?> _getter;

    private bool _showHex;
    private bool _showMultiline;
    private string? _name;
    private Func<object, string>? _formatter;

    internal InspectorPropertyBuilder(MemberInfo member, Type propertyType, Func<object, object?> getter)
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
    
    public InspectorPropertyMapping Build()
    {
        var displayName = _name ?? _member.Name.ToNaturalWording();
        return new InspectorPropertyMapping(displayName, _member, _propertyType, _getter, _showHex,
            _showMultiline, _formatter);
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

    private static MemberInfo GetMemberFromExpression(Expression<Func<T, TProp>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            body = unaryExpression.Operand as MemberExpression;
        }

        if (body is MemberExpression memberExpression)
        {
            return memberExpression.Member;
        }

        throw new ArgumentException("Expression must be a simple member accessor");
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
}