namespace Display.Extensions;

internal static class String
{
    public static long? ToNullableLong(this string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return long.TryParse(value, out var longResult) ? longResult : null;
    }

    public static long ToLong(this string value)
    {
        return long.TryParse(value, out var longResult) ? longResult : 0;
    }


    /// <summary>
    /// 字符串内容是否为数字
    /// </summary>
    /// <param name="string"></param>
    /// <returns></returns>
    public static bool IsNumber(this string @string)
    {
        if (string.IsNullOrEmpty(@string))
            return false;

        foreach (char c in @string)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
}