using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// true => Visible;
/// false => Collapsed
/// </summary>
public class TrueToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
