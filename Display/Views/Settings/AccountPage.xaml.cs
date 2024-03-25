using Display.CustomWindows;
using Display.Helper.FileProperties.Name;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Windows.ApplicationModel.DataTransfer;
using UserInfo = Display.Controls.UserInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AccountPage : RootPage
{
    private static readonly WebApi WebApi = WebApi.GlobalWebApi;

    public AccountPage()
    {
        this.InitializeComponent();

        InitializationViewAsync();
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

    private void UpdateLoginStatus()
    {
        UserInfoControl.Status = WebApi.UserInfo?.state == true ? UserInfo.LoginStatus.Login : UserInfo.LoginStatus.NoLogin;
    }

    private void UpdateUserInfo()
    {
        UserInfoControl.Userinfo = WebApi.UserInfo?.data;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UpdateInfoButton_Click(object sender, RoutedEventArgs e)
    {
        UserInfoControl.Status = UserInfo.LoginStatus.Update;
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
    /// <param name="sender"></param>
    /// <param name="args"></param>

    private void LoginWindow_Closed(object sender, WindowEventArgs args)
    {
        InitializationViewAsync();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        WebApi.LogoutAccount();
        UserInfoControl.Status = UserInfo.LoginStatus.NoLogin;
    }

    #region cookie



    /// <summary>
    /// 删除cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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


    /// <summary>
    /// 复制cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CopyCookieButtonClick(object sender, RoutedEventArgs e)
    {
        //创建一个数据包
        var dataPackage = new DataPackage();
        dataPackage.SetText(CookieBox.Password);

        //把数据包放到剪贴板里
        Clipboard.SetContent(dataPackage);

        ShowTeachingTip("已复制");
    }

    /// <summary>
    /// 导出Cookies
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
    /// 显示或隐藏Cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Show115CookieButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton button) return;

        CookieBox.PasswordRevealMode = button.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
    }


    private void ClearDownRecordButton_Click(object sender, RoutedEventArgs e)
    {
        DataAccess.Delete.DeleteTable(DataAccess.TableName.DownHistory);

        ShowTeachingTip("已清空");
    }


    #endregion

}