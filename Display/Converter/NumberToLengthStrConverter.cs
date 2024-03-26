
using Display.Extensions;
using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

public class NumberToLengthStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            int intValue => DateHelper.ConvertDoubleToLengthStr(intValue),
            double doubleValue => DateHelper.ConvertDoubleToLengthStr(doubleValue),
            string numOrStringValue when !numOrStringValue.IsNumber() => numOrStringValue,
            string numOrStringValue => DateHelper.ConvertDoubleToLengthStr(int.Parse(numOrStringValue)),
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
