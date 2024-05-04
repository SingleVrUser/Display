using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Helper.Data;
using Display.Interfaces;
using Display.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Display.Helper.Network;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo.OneOneFive;
using Display.Providers.Downloader;
using LocalThumbnail = Display.Models.Dto.Media.LocalThumbnail;

namespace Display.ViewModels;

internal partial class ThumbnailViewModel(IThumbnailGeneratorService thumbnailGeneratorService) : ObservableObject
{
    private string _videoUrl;
    private List<DetailFileInfo> _fileInfos;

    [ObservableProperty]
    private bool _loading;

    internal readonly ObservableCollection<GroupThumbnailCollection> ThumbnailList = [];

    [ObservableProperty]
    private DetailFileInfo _currentDetailFileItem;

    [ObservableProperty]
    private LocalThumbnail _currentThumbnailItem;

    private readonly WebApi _webApi = WebApi.GlobalWebApi;

    public void SetCurrentItem(object dst)
    {
        for (var i = 0; i < ThumbnailList.Count; i++)
        {
            var collection = ThumbnailList[i];

            if (collection.All(item => dst != item)) continue;

            CurrentThumbnailItem = (LocalThumbnail)dst;
            CurrentDetailFileItem = _fileInfos[i];
            return;
        }
    }

    public void SetData(List<DetailFileInfo> fileInfos)
    {
        _fileInfos = fileInfos;

        fileInfos.ForEach(info =>
        {
            ThumbnailList.Add(new GroupThumbnailCollection(info));
        });
    }

    public async void StartAsync(Action firstItemSuccessAction)
    {
        Loading = true;
        var isFirst = true;
        foreach (var item in ThumbnailList)
        {
            // 获取m3u8链接或者下载链接

            //转码成功，可以用m3u8
            if (item.HasM3U8)
            {
                var m3U8Infos = await _webApi.GetM3U8InfoByPickCode(item.PickCode);

                if (m3U8Infos.Count > 0)
                {
                    _videoUrl = m3U8Infos[0].Url;
                }
            }

            if (string.IsNullOrEmpty(_videoUrl))
            {
                // 视频未转码，m3u8链接为0，尝试获取直链
                var downUrlList = await _webApi.GetDownUrl(item.PickCode, DbNetworkHelper.DownUserAgent);

                if (downUrlList.Count > 0)
                {
                    _videoUrl = downUrlList.FirstOrDefault().Value;
                }
            }

            if (string.IsNullOrEmpty(_videoUrl)) return;

            var thumbnailGenerateOptions = new ThumbnailGenerateOptions
            {
                SavePath = LocalCacheHelper.CachePath,
                StringFormat = item.Title + "-{0}",
                UrlOptions = new UrlOptions
                {
                    Url = _videoUrl,
                    Headers = new Dictionary<string, string>
                    {
                        { "referer", "https://115.com" },
                        { "user_agent",DbNetworkHelper.DownUserAgent }
                    }
                }
            };

            var progress = new Progress<LocalThumbnail>(info =>
            {
                item.Add(info);
                if (!isFirst) return;

                firstItemSuccessAction();
                isFirst = false;
                Loading = false;
            });

            try
            {
                Debug.WriteLine("开始获取缩略图");
                await thumbnailGeneratorService.DecodeAllFramesToImages(thumbnailGenerateOptions, progress);
                Debug.WriteLine("成功获取缩略图");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ThumbnailViewModel捕获到异常", ex.Message);
            }
        }

        // 以防未截取到图片
        Loading = false;
    }

    [RelayCommand]
    private void ClearData()
    {
        _fileInfos = null;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var dst = CurrentThumbnailItem;

        var i = -1;
        foreach (var collection in ThumbnailList)
        {
            i++;

            if (collection.All(item => dst != item)) continue;

            // 从115中删除 
            Debug.WriteLine($"删除{CurrentDetailFileItem.Cid}下的{CurrentDetailFileItem.Id}");
            var deleteResult = await WebApi.GlobalWebApi.DeleteFiles(CurrentDetailFileItem.Cid,
                [CurrentDetailFileItem.Id]);

            if (!deleteResult) return;
            _fileInfos.RemoveAt(i); // 要在ThumbnailList的Remove之前执行
            ThumbnailList.Remove(collection);  // Remove会触发事件

            Debug.WriteLine("成功删除");
            return;

        }
    }
}

public class GroupThumbnailCollection(DetailFileInfo info)
    : ObservableCollection<object>(new ObservableCollection<LocalThumbnail>())
{
    public readonly string PickCode = info.PickCode;
    public readonly string Title = info.NameWithoutExtension;
    public readonly bool HasM3U8 = info.Datum.Vdi != 0;
}



