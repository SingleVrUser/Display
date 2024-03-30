using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// Double => Int
/// </summary>
internal class DoubleToInt32Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            double doubleValue => (int)doubleValue,
            int intValue => intValue,
            _ => 0
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}