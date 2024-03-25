#nullable enable
using Display.Helper.Data;
using Display.Helper.Network;
using Display.Models.Data;
using System;
using System.Threading.Tasks;

namespace Display.Models.Image;

public partial class SubImageModel : BaseImage
{
    public readonly FilesInfo FileInfo;
    private ImageInfo? _imageInfo;

    public bool IsDowning;

    public SubImageModel(FilesInfo fileInfo)
    {
        FileInfo = fileInfo;
    }

    public async Task LoadThumbnailFromInternetAsync(IProgress<int>? progress = null)
    {
        if (Thumbnail is null && FileInfo.PickCode is not null)
        {
            IsDowning = true;

            _imageInfo ??= await NetworkHelper.GetImageInfo(FileInfo.PickCode);
            if (_imageInfo is not { state: true })
            {
                IsDowning = false;
                return;
            }

            if (Thumbnail == null)
            {
                var fileName = _imageInfo.data.file_name;

                var filePath = LocalCacheHelper.GetCacheFilePath(fileName);
                if (filePath is null)
                {
                    var result = await NetworkHelper.GetStreamFromUrl(_imageInfo.data.source_url);
                    if (result is null)
                    {
                        IsDowning = false;
                        return;
                    }

                    var (stream, size) = result;

                    IProgress<long> positionProgress = new Progress<long>((p) =>
                    {
                        progress?.Report((int)((double)p / size * 100));
                    });
                    filePath = await LocalCacheHelper.SaveCacheFilePath(fileName, stream, positionProgress);

                    if (filePath is null)
                    {
                        IsDowning = false;
                        return;
                    }
                }
                else
                {
                    progress?.Report(100);
                    //var fileInfo = new FileInfo(filePath);
                    //Size = fileInfo.Length;
                }

                await SetBitmap(filePath);
                //var bitmapImage = new BitmapImage();

                //var imageFile = await StorageFile.GetFileFromPathAsync(filePath);

                //using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                //{
                //    await bitmapImage.SetSourceAsync(fileStream);
                //}

                ////var properties = await imageFile.Properties.GetImagePropertiesAsync();
                ////Dimensions = $"{properties.Width} x {properties.Height}"; 
                ////Dimensions = $"{bitmapImage.PixelWidth} x {bitmapImage.PixelHeight}";

                //Thumbnail = bitmapImage;
            }

            IsDowning = false;
        }
    }

}