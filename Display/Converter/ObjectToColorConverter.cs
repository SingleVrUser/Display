using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Display.Converter;

/// <summary>
/// object => SolidColorBrush
/// </summary>
internal class ObjectToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var color = Colors.Black;

        if (value == null) return new SolidColorBrush(color);

        var hash = value.GetHashCode();
        var rnd = new Random(hash);
        color = Color.FromArgb(255, (byte)rnd.Next(64, 192), (byte)rnd.Next(64, 192), (byte)rnd.Next(64, 192));
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}