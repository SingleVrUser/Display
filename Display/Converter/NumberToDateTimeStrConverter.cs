
using Display.Extensions;
using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class NumberToDateTimeStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //2022-09-18 17:24 或者 1663493094

        if (value is int intValue)
        {
            return DateHelper.ConvertInt32ToDateTime(intValue);
        }

        if (value is string numOrStringValue)
        {
            if (!numOrStringValue.IsNumber())
                return numOrStringValue;

            return DateHelper.ConvertInt32ToDateTime(int.Parse(numOrStringValue));
        }


        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
