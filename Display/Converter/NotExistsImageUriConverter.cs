using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace Display.Converter;

/// <summary>
/// imagePath is empty or not exists => $"{NoPicturePath}"
/// </summary>
internal class NotExistsImageUriConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string imagePath && File.Exists(imagePath))
        {
            return new Uri(imagePath);
        }

        return new Uri(Constants.FileType.NoPicturePath);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
