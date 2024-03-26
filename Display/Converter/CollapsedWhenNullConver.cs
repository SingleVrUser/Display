using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        switch (value)
        {
            case null:
            case string contentStr when string.IsNullOrEmpty(contentStr):
            case 0:
                return Visibility.Collapsed;
            default:
                return Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
