
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

namespace Display.Helper;

public class PlayVideoHelper
{
    public static async Task PlayVideo(string pickCode, XamlRoot xamlRoot = null, SubInfo subInfo = null, CustomMediaPlayerElement.PlayType playType = CustomMediaPlayerElement.PlayType.success, string trueName = "", Page lastPage = null, int playerSelection = -1)
    {
        //115Cookie未空
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

        //是否需要加载字幕
        if (AppSettings.IsFindSub && subInfo == null)
        {
            var subDict = DataAccess.FindSubFile(pickCode);

            if (subDict.Count == 1)
            {
                string subFilePickCode = subDict.First().Key.ToString();
                string subFileName = subDict.First().Value.ToString();
                subInfo = new(subFilePickCode, subFileName, pickCode, trueName);
            }
            else if (subDict.Count > 1 && xamlRoot != null)
            {
                List<SubInfo> subInfos = new();
                subDict.ToList().ForEach(item => subInfos.Add(new(item.Key, item.Value, pickCode, trueName)));

                //按名称排序
                subInfos = subInfos.OrderBy(item => item.name).ToList();

                ContentsPage.DetailInfo.SelectSingleSubFileToSelected newPage = new(subInfos, trueName);
                newPage.ContentListView.ItemClick += SubInfoListView_ItemClick;

                ContentDialog dialog = new()
                {
                    XamlRoot = xamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "选择字幕",
                    PrimaryButtonText = "直接播放",
                    CloseButtonText = "返回",
                    Content = newPage,
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await dialog.ShowAsync();

                //返回
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
            }
        }

        if (playerSelection == -1) playerSelection = AppSettings.PlayerSelection;

        //选择播放器播放
        switch (playerSelection)
        {
            //浏览器播放
            case 0:
                VideoPlayWindow.createNewWindow(FileMatch.getVideoPlayUrl(pickCode));
                break;
            //PotPlayer播放
            case 1:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(pickCode, WebApi.PlayMethod.pot, xamlRoot, subInfo);
                break;
            //mpv播放
            case 2:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(pickCode, WebApi.PlayMethod.mpv, xamlRoot, subInfo);
                break;
            //vlc播放
            case 3:
                await WebApi.GlobalWebApi.PlayVideoWithPlayer(pickCode, WebApi.PlayMethod.vlc, xamlRoot, subInfo);
                break;
            //MediaElement播放
            case 4:
                MediaPlayWindow.CreateNewWindow(pickCode, playType, trueName, lastPage, subInfo);
                break;
        }
    }

    private static async void SubInfoListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not SubInfo singleVideoInfo) return;

        if (sender is not ListView { DataContext: string trueName }) return;

        await PlayVideo(singleVideoInfo.fileBelongPickcode, subInfo: singleVideoInfo, trueName: trueName, lastPage: DetailInfoPage.Current);
    }

}
