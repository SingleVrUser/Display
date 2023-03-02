using Data;
using Display.Control;
using Display.WindowView;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Data.WebApi;
using Display.Views;

namespace Display.Helper;

public class PlayeVideoHelper
{

    public async static Task PlayeVideo(string pickCode, XamlRoot xamlRoot = null, SubInfo subInfo = null, CustomMediaPlayerElement.PlayType playType = CustomMediaPlayerElement.PlayType.success, string trueName = "", Page lastPage = null)
    {
        //115Cookie未空
        if (string.IsNullOrEmpty(AppSettings._115_Cookie) && xamlRoot != null)
        {
            ContentDialog dialog = new ContentDialog()
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

                ContentDialog dialog = new();
                dialog.XamlRoot = xamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "选择字幕";
                dialog.PrimaryButtonText = "直接播放";
                dialog.CloseButtonText = "返回";
                dialog.Content = newPage;
                dialog.DefaultButton = ContentDialogButton.Close;

                var result = await dialog.ShowAsync();

                //返回
                switch (result)
                {
                    //直接播放
                    case ContentDialogResult.Primary:

                        break;

                    default:
                        return;
                }
            }
        }

        //选择播放器播放
        switch (AppSettings.PlayerSelection)
        {
            //浏览器播放
            case 0:
                VideoPlayWindow.createNewWindow(FileMatch.getVideoPlayUrl(pickCode));
                break;
            //PotPlayer播放
            case 1:
                WebApi webapi = new();
                await webapi.PlayVideoWithOriginUrl(pickCode, playMethod.pot, xamlRoot, subInfo);
                break;
            //mpv播放
            case 2:
                webapi = new();
                await webapi.PlayVideoWithOriginUrl(pickCode, playMethod.mpv, xamlRoot, subInfo);
                break;
            //vlc播放
            case 3:
                webapi = new();
                await webapi.PlayVideoWithOriginUrl(pickCode, playMethod.vlc, xamlRoot, subInfo);
                break;
            //MediaElement播放
            case 4:
                MediaPlayWindow.CreateNewWindow(pickCode, playType, trueName, lastPage);
                break;
        }
    }

    private async static void SubInfoListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var SingleVideoInfo = e.ClickedItem as Data.SubInfo;

        if (sender is not ListView listView) return;
        if (listView.DataContext is not string trueName) return;

        await PlayeVideo(SingleVideoInfo.fileBelongPickcode, subInfo: SingleVideoInfo, trueName: trueName, lastPage: DetailInfoPage.Current);
    }

}
