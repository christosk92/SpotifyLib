using System;
using System.Linq;
using System.Reflection;
using GuardAgainstLib;

namespace SpotifyLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringAttribute : Attribute
    {
        public StringAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
#nullable enable
        public static bool GetValue(Type enumType, Enum enumValue, out string? result)
        {
            GuardAgainst.ArgumentBeingNull(enumType, nameof(enumType));
            GuardAgainst.ArgumentBeingNull(enumValue, nameof(enumValue));

            if (enumType
              .GetMember(enumValue.ToString())[0]
              .GetCustomAttributes(typeof(StringAttribute))
              .FirstOrDefault() is StringAttribute stringAttr)
            {
                result = stringAttr.Value;
                return true;
            }
            result = null;
            return false;
        }
#nullable disable
    }
}