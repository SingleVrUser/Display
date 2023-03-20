using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class IsVideoToBoolConverter : IValueConverter
{
    public Object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int result && result == 1)
        {
            return true;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
