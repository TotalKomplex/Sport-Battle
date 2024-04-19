using System.Reflection;
using Swen1Csharp.httpserver.http;

public static class EnumExtensions
{
    public static string GetStringValue(this Enum value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
        StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
        return attributes != null && attributes.Length > 0 ? attributes[0].Value : null;
    }
}