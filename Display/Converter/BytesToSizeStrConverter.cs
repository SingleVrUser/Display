using ByteSizeLib;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// bytes => xx.x MB/KB
/// </summary>
internal class BytesToSizeStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || !targetType.IsAssignableFrom(typeof(string)))
        {
            return string.Empty;
        }

        if (value.ToString() == "0")
        {
            return string.Empty;
        }

        var size = value switch
        {
            double doubleValue => ByteSize.FromBytes(doubleValue),
            long longValue => ByteSize.FromBytes(longValue),
            _ => new ByteSize(0)
        };

        return size.ToString("#.#");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
