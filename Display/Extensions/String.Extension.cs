using ByteSizeLib;
using System;

namespace Display.Extensions
{
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
    }
}
