using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InspectMaskedAttribute : Attribute
{
    public char MaskCharacter { get; set; }

    public InspectMaskedAttribute(char maskCharacter = '●')
    {
        MaskCharacter = maskCharacter;
    }
}