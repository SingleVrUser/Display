#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using Display.Helper;
using Display.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Display.ViewModels
{
    public partial class SubImageViewModel :ObservableObject
    {
        private readonly string? _pickCode;
        private ImageInfo? _imageInfo;

        public long Size;
        public bool IsDowning;

        [ObservableProperty]
        private string? _fileName;

        [ObservableProperty]
        private BitmapImage? _thumbnail;

        public SubImageViewModel(string pickCode)
        {
            _pickCode = pickCode;
        }
            
        public async Task LoadThumbnailFromInternetAsync(IProgress<int>? progress = null)
        {
            if (Thumbnail is null && _pickCode is not null)
            {
                IsDowning = true;

                _imageInfo ??= await ImageHelper.GetImageInfo(_pickCode);
                if (_imageInfo is not { state: true })
                {
                    IsDowning = false;
                    return;
                }

                FileName = _imageInfo.data.file_name;

                var filePath = LocalCacheHelper.GetCacheFilePath(FileName);
                if (filePath is null)
                {
                    var result = await ImageHelper.GetStreamFromUrl(_imageInfo.data.source_url);
                    if (result is null)
                    {
                        IsDowning = false;
                        return;
                    }

                    (var stream, Size) = result;

                    IProgress<long> positionProgress = new Progress<long>((p) =>
                    {
                        progress?.Report((int)((double)p / Size * 100));
                    });
                    filePath = await LocalCacheHelper.SaveCacheFilePath(FileName, stream, positionProgress);

                    if (filePath is null)
                    {
                        IsDowning = false;
                        return;
                    }
                }
                else
                {
                    progress?.Report(100);
                    var fileInfo = new FileInfo(filePath);
                    Size = fileInfo.Length;
                }

                var uri = new Uri(filePath);

                Thumbnail = new BitmapImage(uri);

                IsDowning = false;
            }
        }

    }
}
