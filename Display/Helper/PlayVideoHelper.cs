
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
using Display.ContentsPage.DetailInfo;

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
        var firstPlayItem = playItems?.FirstOrDefault();
        if(firstPlayItem == null) return;

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
                VideoPlayWindow.CreateNewWindow(FileMatch.getVideoPlayUrl(firstPlayItem.PickCode));
                break;
            //PotPlayer播放
            case 1:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItems, WebApi.PlayMethod.pot, xamlRoot);
                break;
            //mpv播放
            case 2:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItems, WebApi.PlayMethod.mpv, xamlRoot);
                break;
            //vlc播放
            case 3:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(playItems, WebApi.PlayMethod.vlc, xamlRoot);
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

        var mediaPlayItem = new MediaPlayItem(singleVideoInfo.FileBelongPickCode, trueName, FilesInfo.FileType.File) { SubInfos = new List<SubInfo>(){ singleVideoInfo } };
        await PlayVideo(new List<MediaPlayItem>() { mediaPlayItem }, lastPage: DetailInfoPage.Current);
    }


    public static async void ShowSelectedVideoToPlayPage(List<Datum> multisetList, string trueName, XamlRoot xamlRoot)
    {
        //var multisetList = videoInfoList.ToList();
        multisetList = multisetList.OrderBy(item => item.n).ToList();

        var newPage = new SelectVideoToPlay(multisetList);

        ContentDialog dialog = new()
        {
            XamlRoot = xamlRoot,
            Content = newPage,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "播放",
            PrimaryButtonText = "播放全部",
            SecondaryButtonText = "播放选中项",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
            newPage.PlayAllVideos();
        else if (result == ContentDialogResult.Secondary)
            newPage.PlaySelectedVideos();

    }

}
