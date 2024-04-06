using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;

namespace Display.Converter;

/// <summary>
/// imagePath is empty or not exists => new Bitmap(new Uri($"{NoPicturePath}"))
/// </summary>
internal class NotExistsImageBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        ImageSource imageSource;
        if (value is string imagePath && File.Exists(imagePath))
        {
            imageSource = new BitmapImage(new Uri(imagePath));

            return imageSource;
        }

        imageSource = new BitmapImage(new Uri(Constants.FileType.NoPicturePath));

        return imageSource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
