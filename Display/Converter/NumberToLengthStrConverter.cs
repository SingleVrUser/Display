
using Display.Extensions;
using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// 59 => 59秒
/// </summary>
internal class NumberToLengthStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            int intValue => DateHelper.ConvertDoubleToLengthStr(intValue),
            double doubleValue => DateHelper.ConvertDoubleToLengthStr(doubleValue),
            string stringValue when !stringValue.IsNumber() => stringValue,
            string numValue => DateHelper.ConvertDoubleToLengthStr(int.Parse(numValue)),
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
