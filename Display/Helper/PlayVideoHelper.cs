
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
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using static Display.Data.WebApi;
using Windows.System;

namespace Display.Helper;

public class PlayVideoHelper
{
    /// <summary>
    /// 指定播放器播放
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="xamlRoot"></param>
    /// <param name="playType"></param>
    /// <param name="lastPage"></param>
    /// <param name="playerSelection"></param>
    /// <returns></returns>
    public static async Task PlayVideo(List<MediaPlayItem> playItems, XamlRoot xamlRoot = null, CustomMediaPlayerElement.PlayType playType = CustomMediaPlayerElement.PlayType.Success, Page lastPage = null, int playerSelection = -1)
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
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
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
                VideoPlayWindow.CreateNewWindow(FileMatch.GetVideoPlayUrl(firstPlayItem.PickCode));
                break;
            //PotPlayer播放
            case 1:
                await GlobalWebApi.PlayVideoWithPlayer(playItems, PlayMethod.Pot, xamlRoot);
                break;
            //mpv播放
            case 2:
                await GlobalWebApi.PlayVideoWithPlayer(playItems, PlayMethod.Mpv, xamlRoot);
                break;
            //vlc播放
            case 3:
                await GlobalWebApi.PlayVideoWithPlayer(playItems, PlayMethod.Vlc, xamlRoot);
                break;
            //MediaElement播放
            case 4:
                MediaPlayWindow.CreateNewWindow(playItems, playType, lastPage);
                break;
        }
    }

    /// <summary>
    /// PotPlayer播放（原画）
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="userAgent"></param>
    /// <param name="fileName"></param>
    /// <param name="quality"></param>
    /// <param name="showWindow"></param>
    /// <param name="referrerUrl"></param>
    public static async void Play115SourceVideoWithPotPlayer(List<MediaPlayItem> playItems, string userAgent, string fileName, Const.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
    {
        var isFirst = true;
        foreach (var mediaPlayItem in playItems)
        {
            var downUrl = await mediaPlayItem.GetUrl(quality);
            var subFile = await mediaPlayItem.GetOneSubFilePath();
            var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" /sub=""{subFile}""";
            var arguments = @$" ""{downUrl}"" /user_agent=""{userAgent}"" /referer=""{referrerUrl}""{addSubFile}";

            if (isFirst)
            {
                isFirst = false;
                arguments += " /current";
                StartProcess(fileName, arguments, exitedHandler: (_,_) =>
                {
                    Debug.WriteLine("Process_Exited");
                });
                await Task.Delay(10000);
            }
            else
            {
                arguments += " /add";
                StartProcess(fileName, arguments, showWindow);
                await Task.Delay(1000);
            }
        }
    }

    /// <summary>
    /// mpv播放
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="userAgent"></param>
    /// <param name="fileName"></param>
    /// <param name="quality"></param>
    /// <param name="showWindow"></param>
    /// <param name="referrerUrl"></param>
    public static async void Play115SourceVideoWithMpv(List<MediaPlayItem> playItems, string userAgent, string fileName, Const.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
    {
        var arguments = string.Empty;
        foreach (var mediaPlayItem in playItems)
        {
            var playUrl = await mediaPlayItem.GetUrl(quality);
            var title = mediaPlayItem.Title;
            var subFile = await mediaPlayItem.GetOneSubFilePath();

            var addTitle = string.Empty;
            if (string.IsNullOrEmpty(title))
            {
                var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                if (matchTitle.Success)
                    title = matchTitle.Groups[1].Value;
            }

            if (!string.IsNullOrEmpty(title))
                addTitle = @$"  --title=""播放 - {title}""";

            var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" --sub-file=""{subFile}""";

            arguments += " --{" + @$" ""{playUrl}"" --referrer=""{referrerUrl}"" --user-agent=""{userAgent}"" --force-media-title=""{mediaPlayItem.FileName}""{addTitle}{addSubFile}" + " --}";
        }

        StartProcess(fileName, arguments, showWindow);

    }

    /// <summary>
    /// vlc播放（原画）
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="userAgent"></param>
    /// <param name="fileName"></param>
    /// <param name="quality"></param>
    /// <param name="showWindow"></param>
    /// <param name="referrerUrl"></param>
    public static async void Play115SourceVideoWithVlc(List<MediaPlayItem> playItems, string userAgent, string fileName, Const.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
    {
        var arguments = string.Empty;
        foreach (var mediaPlayItem in playItems)
        {
            var playUrl = await mediaPlayItem.GetUrl(quality);
            var title = mediaPlayItem.Title;
            var subFile = await mediaPlayItem.GetOneSubFilePath();

            var addTitle = string.Empty;
            if (string.IsNullOrEmpty(title))
            {
                var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                if (matchTitle.Success)
                    addTitle = matchTitle.Groups[1].Value;
            }

            if (!string.IsNullOrEmpty(title))
            {
                addTitle = @$" :meta-title=""{title}""";
            }
            var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" :sub-file=""{subFile}""";

            arguments += @$" ""{playUrl}"" :http-referrer=""{referrerUrl}"" :http-user-agent=""{userAgent}""{addTitle}{addSubFile}";
        }

        StartProcess(fileName, arguments, showWindow);
    }

    /// <summary>
    /// 通过Process调用指定播放器
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <param name="showWindow"></param>
    /// <param name="exitedHandler"></param>
    private static async void StartProcess(string fileName, string arguments, bool showWindow = false, EventHandler exitedHandler = null)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = !showWindow
            }
        };

        if (exitedHandler != null)
        {
            process.Exited += exitedHandler;
        }
        try
        {
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Win32Exception e)
        {
            Toast.TryToast("播放错误", "调用播放器时出现异常", e.Message);
        }
    }


    public static async void PlayByPotPlayer(string playUrl)
    {
        var url = $"PotPlayer://{playUrl}";
        var uriDown = new Uri(url);

        // Set the option to show a warning
        var options = new LauncherOptions
        {
            DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseLess
        };

        // Launch the URI
        await Launcher.LaunchUriAsync(uriDown, options);
    }


    public static async void ShowSelectedVideoToPlayPage(List<Datum> multisetList, XamlRoot xamlRoot)
    {
        //var multisetList = videoInfoList.ToList();
        multisetList = multisetList.OrderBy(item => item.Name).ToList();

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
