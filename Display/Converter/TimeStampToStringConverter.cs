using Display.Helper.Date;
using Microsoft.UI.Xaml.Data;
using System;

namespace Display.Converter;

/// <summary>
/// 70 => 00:01:10
/// </summary>
internal class TimeStampToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not long timeStamp) return "00";

        DateHelper.GetTimeFromTimeStamp(timeStamp, out var hh, out var mm, out var ss);
        return $"{hh:D2}:{mm:D2}:{ss:D2}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}