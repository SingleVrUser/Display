
using Display.Extensions;
using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class NumberToLengthStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {

        if (value is int intValue)
        {
            return DateHelper.ConvertDoubleToLengthStr(intValue);
        }

        if (value is double doubleValue)
        {
            return DateHelper.ConvertDoubleToLengthStr(doubleValue);
        }

        if (value is string numOrStringValue)
        {
            if (!numOrStringValue.IsNumber())
                return numOrStringValue;

            return DateHelper.ConvertDoubleToLengthStr(int.Parse(numOrStringValue));
        }


        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
