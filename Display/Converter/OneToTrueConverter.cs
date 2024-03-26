using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// 1 => true
/// other => false
/// </summary>
internal class OneToTrueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
