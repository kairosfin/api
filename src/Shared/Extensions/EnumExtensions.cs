using System;
using System.ComponentModel;
using System.Reflection;

namespace Kairos.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());

        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) 
                is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }
        }

        return value.ToString();
    }
}
