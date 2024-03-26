#nullable enable
using System;
using System.Threading.Tasks;
using Display.Helper.Data;
using Display.Helper.Network;
using Display.Models.Dto.OneOneFive;

namespace Display.Models.Media;

internal class SubImageModel(FilesInfo fileInfo) : Media.BaseImage
{
    public readonly FilesInfo FileInfo = fileInfo;
    private ImageInfo? _imageInfo;

    public bool IsDowning;

    public async Task LoadThumbnailFromInternetAsync(IProgress<int>? progress = null)
    {
        if (Thumbnail is null && FileInfo.PickCode is not null)
        {
            IsDowning = true;

            _imageInfo ??= await NetworkHelper.GetImageInfo(FileInfo.PickCode);
            if (_imageInfo is not { State: true })
            {
                IsDowning = false;
                return;
            }

            if (Thumbnail == null)
            {
                var fileName = _imageInfo.Data.FileName;

                var filePath = LocalCacheHelper.GetCacheFilePath(fileName);
                if (filePath is null)
                {
                    var result = await NetworkHelper.GetStreamFromUrl(_imageInfo.Data.SourceUrl);
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