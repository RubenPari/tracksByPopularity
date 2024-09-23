using System.Globalization;

namespace tracksByPopularity.src.helpers;

public static class EnumExtensions
{
    public static TEnum? ToEnum<TEnum>(this string value)
        where TEnum : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var pascalCaseValue = CultureInfo
            .CurrentCulture.TextInfo.ToTitleCase(value)
            .Replace("_", "");

        return Enum.TryParse<TEnum>(pascalCaseValue, out var result) ? result : null;
    }
}
