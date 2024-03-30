
using Display.Extensions;
using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// number(string or number format) => "yyyy/MM/dd HH:mm"
/// </summary>
internal class NumberToDateTimeStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //2022-09-18 17:24 或者 1663493094
        return value switch
        {
            int intValue => DateHelper.ConvertInt32ToDateTime(intValue),
            string stringValue when !stringValue.IsNumber() => stringValue,
            string numValue => DateHelper.ConvertInt32ToDateTime(int.Parse(numValue)),
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
