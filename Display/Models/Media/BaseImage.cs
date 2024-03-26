#nullable enable
using System;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Display.Models.Media;

public abstract partial class BaseImage : ObservableObject
{
    public string? Path { get; set; }

    public BitmapImage? Thumbnail;

    [ObservableProperty]
    private string? _dimensions;

    public async Task SetBitmap(string path)
    {
        Path = path;

        Thumbnail = new BitmapImage();
        var imageFile = await StorageFile.GetFileFromPathAsync(path);
        using var fileStream = await imageFile.OpenAsync(FileAccessMode.Read);
        await Thumbnail.SetSourceAsync(fileStream);

        Dimensions = $"{Thumbnail.PixelWidth} x {Thumbnail.PixelHeight}";
    }
}