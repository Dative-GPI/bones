using System;

public static class StringExtensions
{
    public static string ToCamelCase(this string data)
    {
        if (!String.IsNullOrWhiteSpace(data))
            return Char.ToLowerInvariant(data[0]) + data.Substring(1);
        return data;
    }
}