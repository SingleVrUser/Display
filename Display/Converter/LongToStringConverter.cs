using Microsoft.UI.Xaml.Data;
using System;
using Display.Data;

namespace Display.Converter;

public class LongToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return (int)doubleValue;
        }
        else if(value is int intValue)
        {
            return intValue;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
