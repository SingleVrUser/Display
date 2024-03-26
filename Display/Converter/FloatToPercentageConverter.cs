using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// 0.7 => 70%
/// </summary>
internal class FloatToPercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return $"{(float)value * 100:F0}%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}