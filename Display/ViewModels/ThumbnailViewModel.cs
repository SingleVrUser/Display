using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Helper.Data;
using Display.Interfaces;
using Display.Models.Data;
using Display.Models.Image;
using Display.Models.Media;

namespace Display.ViewModels;

partial class ThumbnailViewModel : ObservableObject
{
    private string _videoUrl;
    private List<FilesInfo> _fileInfos;

    [ObservableProperty]
    private bool _loading;

    public ObservableCollection<GroupThumbnailCollection> ThumbnailList = new();

    [ObservableProperty]
    private FilesInfo _currentFileItem;

    [ObservableProperty]
    private LocalThumbnail _currentThumbnailItem;

    private readonly WebApi _webApi = WebApi.GlobalWebApi;
    private readonly IThumbnailGeneratorService _thumbnailService;

    public ThumbnailViewModel(IThumbnailGeneratorService thumbnailGeneratorService)
    {
        _thumbnailService = thumbnailGeneratorService;
    }

    public void SetCurrentItem(object dst)
    {
        for (var i = 0; i < ThumbnailList.Count; i++)
        {
            var collection = ThumbnailList[i];

            if (collection.All(item => dst != item)) continue;

            CurrentThumbnailItem = (LocalThumbnail)dst;
            CurrentFileItem = _fileInfos[i];
            return;
        }
    }

    public void SetData(List<FilesInfo> fileInfos)
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
                var downUrlList = await _webApi.GetDownUrl(item.PickCode, GetInfoFromNetwork.DownUserAgent);

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
                        { "user_agent",GetInfoFromNetwork.DownUserAgent }
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
                await _thumbnailService.DecodeAllFramesToImages(thumbnailGenerateOptions, progress);
                Debug.WriteLine("成功获取缩略图");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ThumbnailViewModel捕获到异常",ex.Message);
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
    private async void DeleteAsync()
    {
        var dst = CurrentThumbnailItem;

        var i = -1;
        foreach (var collection in ThumbnailList)
        {
            i++;

            if (collection.All(item => dst != item)) continue;

            // 从115中删除 
            Debug.WriteLine($"删除{CurrentFileItem.Cid}下的{CurrentFileItem.Id}");
            var deleteResult = CurrentFileItem.Id != null && await WebApi.GlobalWebApi.DeleteFiles(CurrentFileItem.Cid,
                new[] { (long)CurrentFileItem.Id });

            if (!deleteResult) return;
            _fileInfos.RemoveAt(i); // 要在ThumbnailList的Remove之前执行
            ThumbnailList.Remove(collection);  // Remove会触发事件

            Debug.WriteLine("成功删除");
            return;

        }
    }
}

public class GroupThumbnailCollection : ObservableCollection<object>
{
    public readonly string PickCode;
    public readonly string Title;
    public readonly bool HasM3U8;

    public GroupThumbnailCollection(FilesInfo info): base(new ObservableCollection<LocalThumbnail>())
    {
        Title = info.NameWithoutExtension;
        PickCode = info.PickCode;
        HasM3U8 = info.Datum.Vdi != 0;
    }
}



