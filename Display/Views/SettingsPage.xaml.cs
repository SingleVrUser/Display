
using CommunityToolkit.WinUI.Behaviors;
using Display.CustomWindows;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network.Spider;
using Display.Models.Data;
using Display.Views.Settings;
using Display.Views.Settings.Options;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Display.Models.Data.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage: INotifyPropertyChanged
{
    private SavePath[] _savePaths;
    private Player[] _players;
    private List<Spider> _spiders;

    private bool _isLoadSub;
    private bool IsLoadSub
    {
        get => AppSettings.IsFindSub;
        set
        {
            if (_isLoadSub == value) return;
            _isLoadSub = value;
            AppSettings.IsFindSub = value;
        }
    }

    private Player _currentPlayer;
    private Player CurrentPlayer
    {
        get => _currentPlayer;
        set
        {
            if (_currentPlayer == value) return;
            _currentPlayer = value;
            AppSettings.PlayerSelection = _currentPlayer.Own;
            OnPropertyChanged();
        }
    }


    private static readonly WebApi WebApi = WebApi.GlobalWebApi;

    public SettingsPage()
    {
        InitializeComponent();

        InitOptions();

        InitializationViewAsync();
    }

    private void InitOptions()
    {
        void SaveSamePathAction() => ShowTeachingTip("选择目录与原目录相同，未修改");

        // 保存路径
        _savePaths = new SavePath[] {
            new(AppSettings.DataAccessSavePath)
            {
                Name = "数据文件",
                Own = SavePathEnum.Data,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = newPath =>
                    UpdateDataAccessPath(AppSettings.DataAccessSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new(AppSettings.ImageSavePath)
            {
                Name = "封面图片",
                Own = SavePathEnum.CoverImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateCoverImagePath(AppSettings.ImageSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new(AppSettings.ActorInfoSavePath)
            {
                Name = "演员头像",
                Own = SavePathEnum.ActorImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateActorImagePath(AppSettings.ActorInfoSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new(AppSettings.SubSavePath)
            {
                Name = "字幕文件",
                Own = SavePathEnum.Subtitles,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = path => AppSettings.SubSavePath = path,
                SaveSamePathAction = SaveSamePathAction
            }
        };

        // 播放器
        _players = new Player[]
        {
            new()
            {
                Own = PlayerType.WebView,
                Name = "WebView",
                IsNeedPath = false,
                IsLoadSubOptionOn = false,
                IsChangeQualityEnable = false
            },
            new()
            {
                Own = PlayerType.PotPlayer,
                Name = "PotPlayer",
                Path = AppSettings.PotPlayerExePath,
                SavePathAction = path=>AppSettings.PotPlayerExePath = path,
                ResetPathFunc = () => Const.DefaultSettings.Player.ExePath.PotPlayer
            },
            new()
            {
                Own = PlayerType.Vlc,
                Name = "VLC",
                Path = AppSettings.VlcExePath,
                SavePathAction = path=>AppSettings.VlcExePath = path,
                ResetPathFunc = () => Const.DefaultSettings.Player.ExePath.Vlc
            },
            new()
            {
                Own = PlayerType.Mpv,
                Name = "MPV",
                Path = AppSettings.MpvExePath,
                SavePathAction = path=>AppSettings.MpvExePath = path,
                ResetPathFunc = () => Const.DefaultSettings.Player.ExePath.Mpv
            },
            new()
            {
                Own = PlayerType.MediaElement,
                Name = "MediaElement",
                IsNeedPath = false,
            }
        };

        // 当前选择
        CurrentPlayer = _players.FirstOrDefault(player => player.Own == AppSettings.PlayerSelection);

        _spiders = new List<Spider>();
        // 搜刮源
        foreach (var spider in Manager.Spiders)
        {
            _spiders.Add(new Spider(spider)
            {
                SaveCookieAction = cookie=> spider.Cookie = cookie
            });
            
        }

        _spiders.ForEach(i=> Debug.WriteLine(i.Instance.DelayRanges));  
    }


    private async void InitializationViewAsync()
    {
        if (!string.IsNullOrEmpty(AppSettings._115_Cookie) && WebApi.UserInfo == null)
        {
            await WebApi.UpdateLoginInfoAsync();
        }

        UpdateUserInfo();
        UpdateLoginStatus();
        
    }

    private void UpdateUserInfo()
    {
        UserInfoControl.userinfo = WebApi.UserInfo?.data;
    }

    private void UpdateLoginStatus()
    {
        UserInfoControl.status = WebApi.UserInfo?.state == true ? "Login" : "NoLogin";
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        //显示登录窗口
        var loginWindow = new LoginWindow();
        loginWindow.Activate();

        //关闭登录窗口，刷新页面
        loginWindow.Closed += LoginWindow_Closed;
    }

    /// <summary>
    /// 登录窗口关闭后刷新登录状态
    /// </summary>
    /// <param Name="sender"></param>
    /// <param Name="args"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void LoginWindow_Closed(object sender, WindowEventArgs args)
    {
        InitializationViewAsync();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        WebApi.LogoutAccount();
        UserInfoControl.status = "NoLogin";
    }

    private async void DeleteCookieButton(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认删除",
            PrimaryButtonText = "确认",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = "点击确认后将删除应用中存储的Cookie"
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        WebApi.DeleteCookie();
        CookieBox.Password = null;
    }

    private async void UpdateInfoButton_Click(object sender, RoutedEventArgs e)
    {
        UserInfoControl.status = "Update";
        if (await WebApi.UpdateLoginInfoAsync())
        {
            UpdateUserInfo();
        }
        else
        {
            WebApi.UserInfo = null;
        }
        UpdateLoginStatus();

    }

    /// <summary>
    /// 尝试更新图片保存目录
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateCoverImagePath(string srcPath, string dstPath)
    {
        AppSettings.ImageSavePath = dstPath;
        
        //检查数据库的是否需要修改
        string imagePath = DataAccess.Get.GetOneImagePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("图片保存地址修改完成");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // 数据库的图片地址无需修改
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //提醒修改数据文件
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "提醒",
            PrimaryButtonText = "修改",
            CloseButtonText = "不修改"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //修改数据库图片地址
            DataAccess.Update.UpdateAllImagePath(updateImagePathPage.srcPath, updateImagePathPage.dstPath);
            ShowTeachingTip("修改完成，部分修改内容重启后生效");
        }
        else
        {
            ShowTeachingTip("图片保存地址修改完成");
        }
    }

    /// <summary>
    /// 尝试更新演员保存目录
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateActorImagePath(string srcPath, string dstPath)
    {
        //需要修改的地址
        AppSettings.ActorInfoSavePath = dstPath;

        //检查数据库的是否需要修改
        string imagePath = DataAccess.Get.GetOneActorProfilePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("保存地址修改完成");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // 数据库的图片地址无需修改
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //提醒修改数据文件
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "提醒",
            PrimaryButtonText = "修改",
            CloseButtonText = "不修改"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //修改数据库图片地址
            DataAccess.Update.UpdateActorProfilePath(updateImagePathPage.srcPath, updateImagePathPage.dstPath);
            ShowTeachingTip("修改完成，部分修改内容重启后生效");
        }
        else
        {
            ShowTeachingTip("演员保存地址修改完成");
        }
    }

    /**
     * 更新数据库路径，同时更新保存路径中已存在的数据库文件
     */
    private async void UpdateDataAccessPath(string src, string dst)
    {
        AppSettings.DataAccessSavePath = dst;

        await UpdateDataAccessFile(src, dst);
    }

    private async Task UpdateDataAccessFile(string src, string dst)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            Title = "确认修改",
            PrimaryButtonText = "确认",
            CloseButtonText = "不修改",
            DefaultButton = ContentDialogButton.Primary
        };

        //内容
        RichTextBlock textHighlightingRichTextBlock = new();

        Paragraph paragraph = new();
        paragraph.Inlines.Add(new Run { Text = "修改将进行以下操作：" });
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new Run { Text = "复制原数据文件到目标目录（如果目标目录下没有数据文件）" });

        textHighlightingRichTextBlock.Blocks.Add(paragraph);

        dialog.Content = textHighlightingRichTextBlock;

        dialog.Content = textHighlightingRichTextBlock;
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var dstDbFilepath = DataAccess.NewDbPath(dst);

        var textFileExists = "数据文件已存在，未复制原数据文件";
        if (dstDbFilepath != null && !File.Exists(dstDbFilepath))
        {
            File.Copy(DataAccess.NewDbPath(src), dstDbFilepath, false);
            textFileExists = "原数据文件已复制到指定目录";
        }

        RichTextBlock successRichTextBlock = new();
        Paragraph successParagraph = new();
        successParagraph.Inlines.Add(new Run { Text = "数据文件存放目录修改完成，重启后生效。" });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = textFileExists, Foreground = new SolidColorBrush(Colors.OrangeRed) });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = "如需删除原数据文件，请关闭应用后自行删除" });
        successRichTextBlock.Blocks.Add(successParagraph);

        ShowTeachingTip(successRichTextBlock);
        await Task.Delay(3000);
        FileMatch.LaunchFolder(src);
    }

    private void CopyCookieButtonClick(object sender, RoutedEventArgs e)
    {
        //创建一个数据包
        var dataPackage = new DataPackage();
        dataPackage.SetText(CookieBox.Password);

        //把数据包放到剪贴板里
        Clipboard.SetContent(dataPackage);

        ShowTeachingTip("已复制");
    }

    //导出Cookies
    private async void ExportCookieButton(object sender, RoutedEventArgs e)
    {
        var cookies = CookieBox.Password;
        if (string.IsNullOrEmpty(cookies))
        {
            ShowTeachingTip("输入为空");
            return;
        }

        var exportCookieList = FileMatch.ExportCookies(cookies);

        //设置创建包里的文本内容
        var clipboardText = System.Text.Json.JsonSerializer.Serialize(exportCookieList);

        //创建一个数据包
        var dataPackage = new DataPackage();
        dataPackage.SetText(clipboardText);

        //把数据包放到剪贴板里
        Clipboard.SetContent(dataPackage);

        var dataPackageView = Clipboard.GetContent();
        var text = await dataPackageView.GetTextAsync();
        if (text != clipboardText) return;

        ShowTeachingTip("已添加到剪贴板");
    }

    /// <summary>
    /// 如果选中的搜刮源少于一个，则提示
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SpiderToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton) return;

        //至少选择一个搜刮源
        var isOneTurnOn = Manager.Spiders.Any(spider => spider.IsOn);

        if (isOneTurnOn) return;

        ShowTeachingTip("请至少选择一个搜刮源，否则无法正常搜刮");
    }

    private void BitCometSettingsSave_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSettings = CheckBitCometSettingsFormat(BitCometDownApiTextBox.Text);

        if (bitCometSettings == null)
            return;

        AppSettings.BitCometSettings = bitCometSettings;
    }

    private async void BitCometSettingsCheck_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSettings = CheckBitCometSettingsFormat(BitCometDownApiTextBox.Text);

        if (bitCometSettings == null)
            return;

        BitCometCheckStatus.status = Status.Doing;

        var isOk = await IsBitCometSettingOK(bitCometSettings.UserName, bitCometSettings.Password, bitCometSettings.ApiUrl);

        BitCometCheckStatus.status = isOk ? Status.Success : Status.Error;
    }

    private DownApiSettings CheckBitCometSettingsFormat(string fullApiUrl)
    {
        DownApiSettings bitCometSettings = new();

        if (string.IsNullOrWhiteSpace(fullApiUrl))
        {
            ShowTeachingTip("输入不能为空");

            return null;
        }

        var isMatch = Regex.Match(fullApiUrl, "^(https?://)(\\w+):(\\w+)@([\\w.]+:(\\d+))/?$");

        if (!isMatch.Success)
        {
            ShowTeachingTip("请检查格式是否正确");
            return null;
        }

        bitCometSettings.ApiUrl = $"{isMatch.Groups[1].Value}{isMatch.Groups[4].Value}";
        bitCometSettings.UserName = isMatch.Groups[2].Value;
        bitCometSettings.Password = isMatch.Groups[3].Value;

        return bitCometSettings;
    }

    private async Task<bool> IsBitCometSettingOK(string user, string pwd, string url)
    {
        bool isOK = false;

        var handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            Credentials = new NetworkCredential(user, pwd)
        };

        HttpClient httpClient = new(handler);

        try
        {
            HttpResponseMessage rep = await httpClient.GetAsync(url + "/panel/task_add_httpftp");

            if (rep.IsSuccessStatusCode)
            {
                string content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOK = true;
            }
            else
            {
                if (rep.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ShowTeachingTip("认证失败", "请检查用户名和密码");
                }
            }
        }
        catch (UriFormatException ex)
        {
            //出错
            ShowTeachingTip("网页访问失败", $"{ex.Message}，请检查地址和端口");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return isOK;
    }

    private void ShowTeachingTip(string subtitle, string content = null, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        var notification = new Notification
        {
            Title = subtitle,
            Message = content,
            Severity = severity,
            Duration = TimeSpan.FromSeconds(2)
        };
        NotificationQueue.Show(notification);
    }

    private void ShowTeachingTip(UIElement content, InfoBarSeverity severity = InfoBarSeverity.Informational, int durationSeconds = 2)
    {
        var notification = new Notification
        {
            Title = "提示",
            Content = content,
            Severity = severity
        };

        if (durationSeconds != 0)
        {
            notification.Duration = TimeSpan.FromSeconds(durationSeconds);
        }

        NotificationQueue.Show(notification);
    }

    private void BitCometSavePathOpen_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSavePath = AppSettings.BitCometSavePath;

        if (string.IsNullOrEmpty(bitCometSavePath))
            return;
        FileMatch.LaunchFolder(bitCometSavePath);
    }

    private async void BitCometSavePathChange_Click(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
        folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (folder.Path == AppSettings.BitCometSavePath)
            {
                ShowTeachingTip("选择目录与原目录相同，未修改");
            }
            else
            {
                BitCometSavePathTextBox.Text = folder.Path;

                AppSettings.BitCometSavePath = folder.Path;
            }
        }
    }

    private async void Aria2SettingsCheck_Click(object sender, RoutedEventArgs e)
    {
        DownApiSettings aria2Settings = checkAria2SettingsFormat(Aria2DownApiTextBox.Text);

        if (aria2Settings == null)
            return;

        Aria2CheckStatus.status = Status.Doing;

        bool isOK = await IsAriannaSettingOK(aria2Settings.Password, aria2Settings.ApiUrl);

        if (isOK)
            Aria2CheckStatus.status = Status.Success;
        else
            Aria2CheckStatus.status = Status.Error;
    }

    private void Aria2tSettingsSave_Click(object sender, RoutedEventArgs e)
    {
        DownApiSettings aria2Settings = checkAria2SettingsFormat(Aria2DownApiTextBox.Text);

        if (aria2Settings == null)
            return;

        AppSettings.Aria2Settings = aria2Settings;
    }

    private DownApiSettings checkAria2SettingsFormat(string fullApiUrl)
    {
        DownApiSettings aria2Settings = new();

        if (string.IsNullOrWhiteSpace(fullApiUrl))
        {
            ShowTeachingTip("输入不能为空");

            return null;
        }

        var isMatch = Regex.Match(fullApiUrl, "^https?://(\\w+:\\w+)@[\\w.]+(:\\d+)?/jsonrpc$");

        if (!isMatch.Success)
        {
            ShowTeachingTip("请检查格式是否正确");
            return null;
        }

        aria2Settings.ApiUrl = isMatch.Value;
        //aria2Settings.UserName = isMatch.Groups[2].Value;
        aria2Settings.Password = isMatch.Groups[1].Value;

        return aria2Settings;
    }

    private async Task<bool> IsAriannaSettingOK(string pwd, string url)
    {
        bool isOK = false;

        Aria2Request requclass = new()
        {
            jsonrpc = "2.0",
            method = "aria2.getVersion",
            id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            _params = new string[] { pwd }
        };

        var myContent = JsonConvert.SerializeObject(requclass);

        myContent = myContent.Replace("_params", "params");

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var Content = new StringContent(myContent, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage rep = await httpClient.PostAsync(url, Content);

            if (rep.IsSuccessStatusCode)
            {
                string content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOK = true;
            }
            else
            {
                if (rep.StatusCode == HttpStatusCode.BadRequest)
                {
                    ShowTeachingTip("请求失败", "请检查Secret");
                }
                else if (rep.StatusCode == HttpStatusCode.NotFound)
                {
                    ShowTeachingTip("请求地址有误", "请检查 地址是否以 \"/jsonrpc\" 结尾");
                }
                else if (rep.StatusCode == HttpStatusCode.BadGateway)
                {
                    ShowTeachingTip("端口有误", "请检查端口");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            //出错
            ShowTeachingTip("请求失败", $"{ex.Message}，请检查地址和端口");
        }
        catch (TaskCanceledException ex)
        {
            //出错
            ShowTeachingTip("请求超时", $"{ex.Message}，请检查地址");
        }


        return isOK;
    }

    private async void Aria2SavePathChange_Click(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
        folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (folder.Path == AppSettings.Aria2SavePath)
            {
                ShowTeachingTip("选择目录与原目录相同，未修改");
            }
            else
            {
                Aria2SavePathTextBox.Text = folder.Path;

                AppSettings.Aria2SavePath = folder.Path;
            }
        }
    }

    private void Aria2SavePathOpen_Click(object sender, RoutedEventArgs e)
    {
        var Aria2SavePath = AppSettings.Aria2SavePath;

        if (Aria2SavePath == null)
            return;
        FileMatch.LaunchFolder(Aria2SavePath);
    }

    private void Aria2SavePathClear_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.Aria2SavePath = string.Empty;
        Aria2SavePathTextBox.Text = null;
    }

    private void BitCometSavePathClear_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.BitCometSavePath = string.Empty;
        BitCometSavePathTextBox.Text = null;
    }


    private void ClearDownRecordButton_Click(object sender, RoutedEventArgs e)
    {
        DataAccess.Delete.DeleteTable(DataAccess.TableName.DownHistory);

        ShowTeachingTip("已清空");
    }

    private void X1080XBaseUrlChange(object sender, RoutedEventArgs e)
    {
        AppSettings.X1080XBaseUrl = X1080UrlTextBox.Text;

        ShowTeachingTip("修改完成");
    }

    private void X1080XCookieChange(object sender, RoutedEventArgs e)
    {
        AppSettings.X1080XCookie = X1080XCookieTextBox.Text;

        GetInfoFromNetwork.IsX1080XCookieVisible = true;

        X1080X.TryChangedClientHeader("cookie", AppSettings.X1080XCookie);

        ShowTeachingTip("修改完成");
    }

    private void X1080XUserAgentChange(object sender, RoutedEventArgs e)
    {
        AppSettings.X1080XUa = X1080XuaTextBox.Text;

        X1080X.TryChangedClientHeader("user-agent", AppSettings.X1080XCookie);

        ShowTeachingTip("修改完成");
    }

    private async void Selected115SavePathButtonClick(object sender, RoutedEventArgs e)
    {
        var contentPage = new SelectedFolderPage();

        var result = await contentPage.ShowContentDialogResult(XamlRoot);

        if (result != ContentDialogResult.Primary) return;

        var explorerItem = contentPage.GetCurrentFolder();
        Debug.WriteLine($"当前选中：{explorerItem.Name}({explorerItem.Id})");

        AppSettings.SavePath115Name = explorerItem.Name;

        AppSettings.SavePath115Cid = explorerItem.Id;

        SavePath115NameTextBlock.Text = explorerItem.Name;
        SavePath115CidTextBlock.Text = explorerItem.Id.ToString();

        ShowTeachingTip("设置成功");
    }

    private async void Save115SavePathButtonClick(object sender, RoutedEventArgs e)
    {
        if(!long.TryParse(SavePath115CidTextBlock.Text,out var cid)) return;

        var cidInfo = await WebApi.GetFolderCategory(cid);

        if (cidInfo != null)
        {
            SavePath115NameTextBlock.Text = cidInfo.file_name;
            AppSettings.SavePath115Name = cidInfo.file_name;
            AppSettings.SavePath115Cid = cid;

            ShowTeachingTip("保存成功");
        }
        else
        {
            ShowTeachingTip("保存失败");
        }
    }

    private void Show115CookieButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton button) return;

        CookieBox.PasswordRevealMode = button.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
}