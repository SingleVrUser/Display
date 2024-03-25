using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace Display.Converter;

public class NotExistsImagePathConverter : IValueConverter
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
