using Microsoft.UI.Xaml.Data;
using System;
using Display.Data;

namespace Display.Converter;

public class Int32ToDateTimeStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //2022-09-18 17:24 或者 1663493094

        if (value is int IntValue)
        {
            return FileMatch.ConvertInt32ToDateTime(IntValue);
        }
        else if (value is string NumOrStringValue)
        {
            if (!FileMatch.isNumberic1(NumOrStringValue))
                return NumOrStringValue;

            return FileMatch.ConvertInt32ToDateTime(int.Parse(NumOrStringValue));
        }


        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
