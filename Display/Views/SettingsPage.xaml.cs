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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private WebApi webapi = new();

        public SettingsPage()
        {
            this.InitializeComponent();

            InitializationView();

        }

        private async void InitializationView()
        {
            if (!string.IsNullOrEmpty(AppSettings._115_Cookie) &&WebApi.UserInfo == null)
            {
                await webapi.UpdateLoginInfo();

                //检查一下是否为隐藏模式
                await webapi.IsHiddenModel();
            }

            updateUserInfo();
            updateLoginStatus();

            infobar.IsOpen = WebApi.isEnterHiddenMode == true ? WebApi.isEnterHiddenMode : false;

            //ImageSavePath_TextBox.Text = (string)localSettings.Values["ImageSave_Path"];
            DataAccessSavePath_TextBox.Text = AppSettings.DataAccess_SavePath;

            ////是否存在cookie
            //var cookieText = (string)localSettings.Values["Cookie"];
            //if (cookieText != null)
            //{
            //    Cookie_TextBox.Text = cookieText;
            //}

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

        private async void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            webapi.LogoutAccount();
            await Task.Delay(2000);

            InitializationView();
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
            FolderPicker folderPicker = new();
            folderPicker.FileTypeFilter.Add("*");
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                if (folder.Path == AppSettings.Image_SavePath)
                {
                    LightDismissTeachingTip.Content = "选择目录与原目录相同，未修改";
                    LightDismissTeachingTip.IsOpen = true;
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
                LightDismissTeachingTip.Content = "修改成功，重启后生效";
                LightDismissTeachingTip.IsOpen = true;
            }

        }

        /// <summary>
        /// 打开图片存放目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageOpenPath_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            string ImagePath = AppSettings.Image_SavePath;
            FileMatch.LaunchFolder(ImagePath);
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
        }

        private void JavDBUrlChange_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

            AppSettings.JavDB_BaseUrl = JavDBUrl_TextBox.Text;
        }

        private void JavDBCookieChange_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            AppSettings.javdb_Cookie = JavDBCookie_TextBox.Text;
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
                    LightDismissTeachingTip.Content = "选择目录与原目录相同，未修改";
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
                            File.Copy(DataAccess.dbpath, dstDBFilepath, false);
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
                LightDismissTeachingTip.Content = "输入为空";
                LightDismissTeachingTip.IsOpen = true;
                return;
            }

            var exportCookieList = FileMatch.ExportCookies(coookieText);

            //创建一个数据包
            DataPackage dataPackage = new DataPackage();
            //设置创建包里的文本内容
            string ClipboardText = JsonSerializer.Serialize(exportCookieList);
            dataPackage.SetText(ClipboardText);
            
            //把数据包放到剪贴板里
            Clipboard.SetContent(dataPackage);


            DataPackageView dataPackageView = Clipboard.GetContent();
            string text = await dataPackageView.GetTextAsync();
            if (text == ClipboardText)
            {
                LightDismissTeachingTip.Content = "已添加到剪贴板";
                LightDismissTeachingTip.IsOpen = true;
            }

        }
    }
}
