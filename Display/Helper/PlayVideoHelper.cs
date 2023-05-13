﻿
using Display.Controls;
using Display.Views;
using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Display.Data;
using Display.Models;

namespace Display.Helper;

public class PlayVideoHelper
{
    /// <summary>
    /// 根据指定播放器播放
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="xamlRoot"></param>
    /// <param name="playType"></param>
    /// <param name="lastPage"></param>
    /// <param name="playerSelection"></param>
    /// <returns></returns>
    public static async Task PlayVideo(List<MediaPlayItem> playItems, XamlRoot xamlRoot = null, CustomMediaPlayerElement.PlayType playType = CustomMediaPlayerElement.PlayType.success, Page lastPage = null, int playerSelection = -1)
    {
        // 播放项不能为空
        var playItem = playItems?.FirstOrDefault();
        if(playItem == null) return;

        //115Cookie不能为空
        if (string.IsNullOrEmpty(AppSettings._115_Cookie) && xamlRoot != null)
        {
            var dialog = new ContentDialog()
            {
                XamlRoot = xamlRoot,
                Title = "播放失败",
                CloseButtonText = "返回",
                DefaultButton = ContentDialogButton.Close,
                Content = "115未登录，无法播放视频，请先登录"
            };

            await dialog.ShowAsync();
        }

        if (playerSelection == -1) playerSelection = AppSettings.PlayerSelection;

        //选择播放器播放
        switch (playerSelection)
        {
            //浏览器播放
            case 0:
                VideoPlayWindow.createNewWindow(FileMatch.getVideoPlayUrl(playItem.PickCode));
                break;
            //PotPlayer播放
            case 1:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItem, WebApi.PlayMethod.pot, xamlRoot);
                break;
            //mpv播放
            case 2:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItem, WebApi.PlayMethod.mpv, xamlRoot);
                break;
            //vlc播放
            case 3:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItem, WebApi.PlayMethod.vlc, xamlRoot);
                break;
            //MediaElement播放
            case 4:
                MediaPlayWindow.CreateNewWindow(playItems, playType, lastPage);
                break;
        }
    }

    public static async void SubInfoListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not SubInfo singleVideoInfo) return;

        if (sender is not ListView { DataContext: string trueName }) return;

        var mediaPlayItem = new MediaPlayItem(singleVideoInfo.fileBelongPickcode, trueName) { SubInfos = new List<SubInfo>(){ singleVideoInfo } };
        await PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, lastPage: DetailInfoPage.Current);
    }

}
