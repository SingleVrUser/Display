using Microsoft.UI.Xaml.Controls;
using System;
using Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Data;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using System.Text.Json;
using System.IO;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.Media.Protection.PlayReady;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        ObservableCollection<string> resolutionSelectionCollection = new ObservableCollection<string>();

        ObservableCollection<string> SpiderMothodList = new();

        private WebApi webapi;

        public SettingsPage()
        {
            this.InitializeComponent();

            InitializationView();

        }

        private async void InitializationView()
        {
            webapi = new();
            if (!string.IsNullOrEmpty(AppSettings._115_Cookie) && WebApi.UserInfo == null)
            {
                var isLogin = await webapi.UpdateLoginInfo();

                if (isLogin)
                {
                    //检查一下是否为隐藏模式
                    await webapi.IsHiddenModel();
                }
            }

            updateUserInfo();
            updateLoginStatus();

            infobar.IsOpen = WebApi.isEnterHiddenMode == true ? false : true;

            DataAccessSavePath_TextBox.Text = AppSettings.DataAccess_SavePath;
        }

        ///// <summary>
        ///// 检查Cookie是否可用
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void CheckAvailableButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        //{
        //    //状态图标初始化
        //    CheckMarkIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        //    ErrorIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        //    Cookie_StateProgressRing.IsActive = true;

        //    if (await webapi.UpdateLoginInfo())
        //    {
        //        CheckMarkIcon.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        //    }
        //    else
        //    {
        //        ErrorIcon.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        //    }

        //    Cookie_StateProgressRing.IsActive = false;
        //}

        private void updateUserInfo()
        {
            userInfoControl.userinfo = WebApi.UserInfo == null ? null: WebApi.UserInfo.data;
        }

        private void updateLoginStatus()
        {
            userInfoControl.status = WebApi.UserInfo == null ? "NoLogin" : "Login";
        }

        //private void update_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        //{
        //    userInfoControl.status = "Update";
        //}

        private void LoginButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //显示登录窗口
            WindowView.LoginWindow loginWindow = new WindowView.LoginWindow();
            loginWindow.Activate();

            //关闭登录窗口，刷新页面
            loginWindow.Closed += LoginWindow_Closed;
        }

        /// <summary>
        /// 登录窗口关闭后刷新登录状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LoginWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
        {
            InitializationView();
        }

        private void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            webapi.LogoutAccount();
            //await Task.Delay(2000);
            userInfoControl.status = "NoLogin";
            infobar.Visibility = Visibility.Collapsed;
            //InitializationView();
        }

        private async void DeleteCookieButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "确认删除";
            dialog.PrimaryButtonText = "确认";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = "点击确认后将删除应用中存储的Cookie";

            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                WebApi.DeleteCookie();
                Cookie_TextBox.Text = null;
            }

        }

        private async void UpdateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            userInfoControl.status = "Update";
            if (await webapi.UpdateLoginInfo())
            {
                updateUserInfo();
                infobar.IsOpen = await webapi.IsHiddenModel();
            }
            else
            {
                WebApi.UserInfo = null;
            }
            updateLoginStatus();

        }

        private async void ImageSavePath_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var folder = await OpenFolder(PickerLocationId.PicturesLibrary);

            if (folder != null)
            {
                if (folder.Path == AppSettings.Image_SavePath)
                {
                    ShowTeachingTip("选择目录与原目录相同，未修改");
                }
                else
                {
                    ImageSavePath_TextBox.Text = folder.Path;
                    tryUpdateImagePath(folder.Path);
                }
            }
        }
        /// <summary>
        /// 尝试更新图片保存目录
        /// </summary>
        /// <param name="folderPath"></param>
        private async void tryUpdateImagePath(string folderPath)
        {
            string srcPath = AppSettings.Image_SavePath;
            string dstPath = folderPath;
            string imagePath = DataAccess.GetOneImagePath();
            if (string.IsNullOrEmpty(imagePath)) return;

            AppSettings.Image_SavePath = folderPath;

            var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
            var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

            // 数据库的图片地址无需修改
            if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;
            //if (imagePath.Replace(srcPath, dstPath) == imagePath) return;

            //提醒修改数据文件
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "提醒";
            dialog.PrimaryButtonText = "修改";
            dialog.CloseButtonText = "不修改";
            //dialog.DefaultButton = ContentDialogButton.Primary;

            var updateImagePathPage = new ContentsPage.UpdateImagePath(imagePath, srcPath, dstPath);
            dialog.Content = updateImagePathPage;

            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                //修改数据库图片地址
                DataAccess.UpdateAllImagePath(updateImagePathPage.srcPath, updateImagePathPage.dstPath);
                ShowTeachingTip("修改成功，重启后生效");
            }

        }



        private async void ActorInfoSavePath_Click(object sender, RoutedEventArgs e)
        {
            var folder = await OpenFolder(PickerLocationId.PicturesLibrary);

            if (folder != null)
            {
                if (folder.Path == AppSettings.Image_SavePath)
                {
                    ShowTeachingTip("选择目录与原目录相同，未修改");
                }
                else
                {
                    //修改地址
                    ActorSavePath_TextBox.Text = folder.Path;
                    AppSettings.ActorInfo_SavePath = folder.Path;

                }
            }
        }


        private async void SubSavePath_Click(object sender, RoutedEventArgs e)
        {
            var folder = await OpenFolder(PickerLocationId.PicturesLibrary);

            if (folder != null)
            {
                if (folder.Path == AppSettings.Sub_SavePath)
                {
                    ShowTeachingTip("选择目录与原目录相同，未修改");
                }
                else
                {
                    SubSavePath_TextBox.Text = folder.Path;
                    AppSettings.Sub_SavePath = folder.Path;
                }
            }
        }

        /// <summary>
        /// 打开字幕文件的存放路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubOpenPath_Click(object sender, RoutedEventArgs e)
        {
            FileMatch.LaunchFolder(AppSettings.Sub_SavePath);
        }

        /// <summary>
        /// 打开演员信息存放路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActorInfoOpenPath_Click(object sender, RoutedEventArgs e)
        {
            FileMatch.LaunchFolder(AppSettings.ActorInfo_SavePath);
        }

        /// <summary>
        /// 打开图片存放目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageOpenPath_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            FileMatch.LaunchFolder(AppSettings.Image_SavePath);
        }

        /// <summary>
        /// 打开数据库目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataAccessOpenPath_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            FileMatch.LaunchFolder(AppSettings.DataAccess_SavePath);
        }

        private void JavbusUrlChange_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            AppSettings.JavBus_BaseUrl = JavbusUrl_TextBox.Text;

            ShowTeachingTip("修改完成");
        }
        private void LibreDmmUrlChange_Button_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.LibreDmm_BaseUrl = LibreDmm_TextBox.Text;

            ShowTeachingTip("修改完成");
        }
        private void Fc2hubUrlChange_Button_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Fc2hub_BaseUrl = Fc2hub_TextBox.Text;

            ShowTeachingTip("修改完成");
        }
        private void JavDBUrlChange_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            AppSettings.JavDB_BaseUrl = JavDBUrl_TextBox.Text;

            ShowTeachingTip("修改完成");
        }

        private void JavDBCookieChange_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            AppSettings.javdb_Cookie = JavDBCookie_TextBox.Text;

            ShowTeachingTip("修改完成");
        }



        private async void DataAccessSavePath_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            FolderPicker folderPicker = new();
            folderPicker.FileTypeFilter.Add("*");
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                var lastDBSavePath = AppSettings.DataAccess_SavePath;
                if (folder.Path == lastDBSavePath)
                {
                    LightDismissTeachingTip.Subtitle = "选择目录与原目录相同，未修改";
                    LightDismissTeachingTip.IsOpen = true;
                }
                else
                {
                    DataAccessSavePath_TextBox.Text = folder.Path;
                    AppSettings.DataAccess_SavePath = folder.Path;

                    ContentDialog dialog = new();
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.Title = "确认修改";
                    dialog.PrimaryButtonText = "确认";
                    dialog.CloseButtonText = "不修改";
                    dialog.DefaultButton = ContentDialogButton.Primary;

                    //内容
                    RichTextBlock TextHighlightingRichTextBlock = new();

                    Paragraph paragraph = new();
                    paragraph.Inlines.Add(new Run() { Text = "修改将进行以下操作：" });
                    paragraph.Inlines.Add(new LineBreak());
                    paragraph.Inlines.Add(new Run() { Text = "复制原数据文件到目标目录（如果目标目录下没有数据文件）" });


                    TextHighlightingRichTextBlock.Blocks.Add(paragraph);


                    dialog.Content = TextHighlightingRichTextBlock;

                    //TextHighlightingRichTextBlock.TextHighlighters.Add(new TextHighlighter() { Ranges = { new TextRange() } });

                    dialog.Content = TextHighlightingRichTextBlock;
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        var dstDBFilepath = DataAccess.NewDBPath(folder.Path);

                        var textFileExists = "数据文件已存在，未复制原数据文件";
                        if (!File.Exists(dstDBFilepath))
                        {
                            File.Copy(DataAccess.NewDBPath(lastDBSavePath), dstDBFilepath, false);
                            textFileExists = "原数据文件已复制到指定目录";
                        }

                        RichTextBlock SuccessRichTextBlock = new();
                        Paragraph SuccessParagraph = new();
                        SuccessParagraph.Inlines.Add(new Run() { Text = "数据文件存放目录修改完成，重启后生效。" });
                        SuccessParagraph.Inlines.Add(new LineBreak());
                        SuccessParagraph.Inlines.Add(new Run() { Text = textFileExists, Foreground = new SolidColorBrush(Colors.OrangeRed) });
                        SuccessParagraph.Inlines.Add(new LineBreak());
                        SuccessParagraph.Inlines.Add(new Run() { Text = "如需删除原数据文件，请关闭应用后自行删除" });
                        SuccessRichTextBlock.Blocks.Add(SuccessParagraph);

                        LightDismissTeachingTip.Content = SuccessRichTextBlock;
                        ;
                        LightDismissTeachingTip.IsOpen = true;
                        await Task.Delay(3000);
                        FileMatch.LaunchFolder(lastDBSavePath);
                    }
                }


                
            }
        }

        //导出Cookies
        private async void ExportCookieButton(object sender, RoutedEventArgs e)
        {

            var coookieText = Cookie_TextBox.Text;
            if (string.IsNullOrEmpty(coookieText))
            {
                ShowTeachingTip("输入为空");
                return;
            }

            var exportCookieList = FileMatch.ExportCookies(coookieText);

            //创建一个数据包
            DataPackage dataPackage = new DataPackage();
            //设置创建包里的文本内容
            string ClipboardText = System.Text.Json.JsonSerializer.Serialize(exportCookieList);
            dataPackage.SetText(ClipboardText);
            
            //把数据包放到剪贴板里
            Clipboard.SetContent(dataPackage);


            DataPackageView dataPackageView = Clipboard.GetContent();
            string text = await dataPackageView.GetTextAsync();
            if (text == ClipboardText)
            {
                LightDismissTeachingTip.Subtitle = "已添加到剪贴板";
                LightDismissTeachingTip.IsOpen = true;
            }

        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as Microsoft.UI.Xaml.Controls.Primitives.ToggleButton;

            //至少选择一个搜刮源
            bool otherOriginUse = true;
            switch (toggleButton.Content)
            {
                case "JavBus":
                    otherOriginUse = AppSettings.isUseLibreDmm || AppSettings.isUseFc2Hub || AppSettings.isUseJavDB;
                    break;
                case "LibreDmm":
                    otherOriginUse = AppSettings.isUseJavBus || AppSettings.isUseFc2Hub || AppSettings.isUseJavDB;
                    break;
                case "Fc2hub":
                    otherOriginUse = AppSettings.isUseJavBus || AppSettings.isUseLibreDmm || AppSettings.isUseJavDB;
                    break;
                case "JavDB":
                    otherOriginUse = AppSettings.isUseJavBus || AppSettings.isUseLibreDmm || AppSettings.isUseFc2Hub;
                    break;
            }

            if(toggleButton.IsChecked is bool value)
            {
                if (!(value || otherOriginUse))
                {
                    LightDismissTeachingTip.Subtitle = "请至少选择一个搜刮源，否则无法正常搜刮";
                    LightDismissTeachingTip.Target = toggleButton;
                    LightDismissTeachingTip.IsOpen = true;
                }
            }
        }

        private void PlayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                switch (e.AddedItems[0].ToString())
                {
                    case "WebView":
                        Resolution_RelativePanel.Visibility = Visibility.Collapsed;
                        break;

                    case "PotPlayer":
                        resolutionSelectionCollection.Clear();
                        resolutionSelectionCollection.Add("原画");
                        resolutionSelection_ComboBox.SelectedIndex = 0;
                        Resolution_RelativePanel.Visibility = Visibility.Visible;

                        PlayerExePath_RelativePanel.Visibility = Visibility.Visible;

                        PlayerExePath_TextBox.Text = AppSettings.PotPlayerExePath;
                        break;
                    case "mpv":
                        resolutionSelectionCollection.Clear();
                        resolutionSelectionCollection.Add("原画");
                        resolutionSelection_ComboBox.SelectedIndex = 0;
                        Resolution_RelativePanel.Visibility = Visibility.Visible;


                        PlayerExePath_RelativePanel.Visibility = Visibility.Collapsed;

                        //PlayerExePath_TextBox.Text = AppSettings.MpvExePath;
                        break;
                    case "vlc":
                        resolutionSelectionCollection.Clear();
                        resolutionSelectionCollection.Add("原画");
                        resolutionSelection_ComboBox.SelectedIndex = 0;
                        Resolution_RelativePanel.Visibility = Visibility.Visible;

                        PlayerExePath_RelativePanel.Visibility = Visibility.Collapsed;

                        //PlayerExePath_TextBox.Text = AppSettings.VlcExePath;
                        break;
                }
            }
        }

        private void OpenPlayerExePathButton_Click(object sender, RoutedEventArgs e)
        {
            string openPath = string.Empty;

            switch (PlayerSelection_ComboBox.SelectedIndex)
            {
                case 1:
                    openPath = Path.GetDirectoryName(AppSettings.PotPlayerExePath);
                    break;
                //case 2:
                //    openPath = Path.GetDirectoryName(AppSettings.MpvExePath);
                //    break;
                //case 3:
                //    openPath = Path.GetDirectoryName(AppSettings.VlcExePath);
                //    break;
            }

            if(! string.IsNullOrEmpty(openPath))
            {
                FileMatch.LaunchFolder(openPath);
            }

        }

        private async void ModifyPlayerExePathButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new();
            fileOpenPicker.FileTypeFilter.Add(".exe");
            fileOpenPicker.FileTypeFilter.Add(".com");
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

            var file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                switch (PlayerSelection_ComboBox.SelectedIndex)
                {
                    case 1:
                        AppSettings.PotPlayerExePath = file.Path;
                        break;
                    //case 2:
                    //    AppSettings.MpvExePath = file.Path;
                    //    break;  
                    //case 3:
                    //    AppSettings.VlcExePath = file.Path;
                    //    break;
                }
                PlayerExePath_TextBox.Text = file.Path;
            }
        }

        private void BitCometSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            DownApiSettings bitCometSettings = checkBitCometSettingsFormat(BitCometDownApi_TextBox.Text);

            if (bitCometSettings == null)
                return;

            AppSettings.BitCometSettings = bitCometSettings;
        }

        private async void BitCometSettingsCheck_Click(object sender, RoutedEventArgs e)
        {
            DownApiSettings bitCometSettings = checkBitCometSettingsFormat(BitCometDownApi_TextBox.Text);

            if (bitCometSettings == null)
                return;

            BitCometCheckStatus.status = Status.doing;

            bool isOK = await IsBitCometSettingOK(bitCometSettings.UserName, bitCometSettings.Password, bitCometSettings.ApiUrl);

            if(isOK)
                BitCometCheckStatus.status = Status.success;
            else
                BitCometCheckStatus.status = Status.error;

        }

        private DownApiSettings checkBitCometSettingsFormat(string fullApiUrl)
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

        private async Task<bool> IsBitCometSettingOK(string user,string pwd,string url)
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
                    if(rep.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ShowTeachingTip("认证失败","请检查用户名和密码");
                    }
                }
            }
            catch (UriFormatException ex)
            {
                //出错
                ShowTeachingTip("网页访问失败", $"{ex.Message}，请检查地址和端口");
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            return isOK;
        }

        private void ShowTeachingTip(string subtitle,string content=null)
        {
            LightDismissTeachingTip.Subtitle = subtitle;
            if(content != null)
                LightDismissTeachingTip.Content = content;

            LightDismissTeachingTip.IsOpen = true;
        }

        private void BitCometSavePathOpen_Click(object sender, RoutedEventArgs e)
        {
            var BitCometSavePath = AppSettings.BitCometSavePath;

            if (BitCometSavePath == null)
                return;
            FileMatch.LaunchFolder(BitCometSavePath);
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
                    BitCometSavePath_TextBox.Text = folder.Path;

                    AppSettings.BitCometSavePath = folder.Path;
                }
            }
        }

        private async void Aria2SettingsCheck_Click(object sender, RoutedEventArgs e)
        {
            DownApiSettings aria2Settings = checkAria2SettingsFormat(Aria2DownApi_TextBox.Text);

            if (aria2Settings == null)
                return;

            Aria2CheckStatus.status = Status.doing;

            bool isOK = await IsAriannaSettingOK(aria2Settings.Password, aria2Settings.ApiUrl);

            if (isOK)
                Aria2CheckStatus.status = Status.success;
            else
                Aria2CheckStatus.status = Status.error;
        }

        private void Aria2tSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            DownApiSettings aria2Settings = checkAria2SettingsFormat(Aria2DownApi_TextBox.Text);

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

            Aria2Request requclass = new() { jsonrpc = "2.0",
                method = "aria2.getVersion",
                id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                _params = new string[] { pwd } };

            var myContent = JsonConvert.SerializeObject(requclass);

            myContent = myContent.Replace("_params", "params");

            HttpClient httpClient = new HttpClient()
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
                    if (rep.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        ShowTeachingTip("请求失败", "请检查Secret");
                    }
                    else if (rep.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ShowTeachingTip("请求地址有误", "请检查 地址是否以 \"/jsonrpc\" 结尾");
                    }
                    else if (rep.StatusCode == System.Net.HttpStatusCode.BadGateway)
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
                    Aria2SavePath_TextBox.Text = folder.Path;

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
            AppSettings.Aria2SavePath = String.Empty;
            Aria2SavePath_TextBox.Text = null;
        }

        private void BitCometSavePathClear_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.BitCometSavePath = String.Empty;
            BitCometSavePath_TextBox.Text = null;
        }

        public async static Task<StorageFolder> OpenFolder(PickerLocationId SuggestedStartLocation)
        {
            FolderPicker folderPicker = new();
            folderPicker.FileTypeFilter.Add("*");
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
            folderPicker.SuggestedStartLocation = SuggestedStartLocation;

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            return await folderPicker.PickSingleFolderAsync();

        }
    }
}
