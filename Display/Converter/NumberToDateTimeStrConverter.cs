
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
        return value switch
        {
            int intValue => DateHelper.ConvertInt32ToDateTime(intValue),
            string numOrStringValue when !numOrStringValue.IsNumber() => numOrStringValue,
            string numOrStringValue => DateHelper.ConvertInt32ToDateTime(int.Parse(numOrStringValue)),
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
