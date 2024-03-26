using ByteSizeLib;

namespace Display.Extensions;

internal static class ByteSizeExtension
{
    public static string ToByteSizeString(this long value, string format = "0.0")
    {
        var totalSize = ByteSize.FromBytes(value);
        return totalSize.ToString(format);
    }

    public static string ToByteSizeString(this double value, string format = "0.0")
    {
        var totalSize = ByteSize.FromBytes(value);
        return totalSize.ToString(format);
    }
}