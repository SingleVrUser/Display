#nullable enable
using System;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Display.Models.Dto.Media;

public abstract partial class BaseImage : ObservableObject
{
    public string? Path { get; set; }

    public BitmapImage? FullImage;

    [ObservableProperty]
    private string? _dimensions;

    public async Task SetBitmap(string path)
    {
        Path = path;

        FullImage = await getImage(path);
        Dimensions = $"{FullImage.PixelWidth} x {FullImage.PixelHeight}";
    }

    private static async Task<BitmapImage> getImage(string path, int? decodePixelHeight = null)
    {
        var bitmapImage = decodePixelHeight == null ? new BitmapImage(): new BitmapImage()
        {
            DecodePixelHeight = (int)decodePixelHeight
        };

        var imageFile = await StorageFile.GetFileFromPathAsync(path);
        using var fileStream = await imageFile.OpenAsync(FileAccessMode.Read);
        await bitmapImage.SetSourceAsync(fileStream);

        return bitmapImage;
    }
}