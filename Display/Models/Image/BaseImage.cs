#nullable enable
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage;

namespace Display.Models.Image;

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