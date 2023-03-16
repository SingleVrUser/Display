using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class BoolToVisibleConverter : IValueConverter
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
