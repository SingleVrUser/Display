using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class IsVideoToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
