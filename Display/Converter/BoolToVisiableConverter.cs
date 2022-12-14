using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class BoolToVisiableConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isTrue && isTrue)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
