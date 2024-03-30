using System;
using Windows.ApplicationModel.DataTransfer;
using Display.Helper.FileProperties.Name;
using Display.Models.Enums;
using Display.Providers;
using Display.Views.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.Settings;

public sealed partial class AccountPage
{
    private static readonly WebApi WebApi = WebApi.GlobalWebApi;

    public AccountPage()
    {
        InitializeComponent();

        InitializationViewAsync();
    }

    private async void InitializationViewAsync()
    {
        if (!string.IsNullOrEmpty(AppSettings._115_Cookie) && (WebApi.UserInfoResult == null || WebApi.UserInfoResult.Data == null))
        {
            await WebApi.UpdateLoginInfoAsync();
        }
        
        UpdateUserInfo();
        UpdateLoginStatus();

        CookieBox.Password = AppSettings._115_Cookie;
    }

    private void UpdateLoginStatus()
    {
        UserInfoControl.Status = WebApi.UserInfoResult?.State == true ? LoginStatus.Login : LoginStatus.NoLogin;
    }

    private void UpdateUserInfo()
    {
        UserInfoControl.Userinfo = WebApi.UserInfoResult?.Data;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UpdateInfoButton_Click(object sender, RoutedEventArgs e)
    {
        UserInfoControl.Status = LoginStatus.Update;
        if (await WebApi.UpdateLoginInfoAsync())
        {
            UpdateUserInfo();
        }
        else
        {
            WebApi.UserInfoResult = null;
        }
        UpdateLoginStatus();

    }

    /// <summary>
    /// 登录成功后刷新登录状态
    /// </summary>
    private void LoginCompleted()
    {
        InitializationViewAsync();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        WebApi.LogoutAccount();
        UserInfoControl.Status = LoginStatus.NoLogin;
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

    private void ClearDownRecordButton_Click(object sender, RoutedEventArgs e)
    {
        DataAccess.Delete.DeleteTable(DataAccess.TableName.DownHistory);

        ShowTeachingTip("已清空");
    }

    #endregion

    private async void SaveCookieClick(object sender, RoutedEventArgs e)
    {
        var newCookie = CookieBox.Password;
        if (string.IsNullOrEmpty(newCookie))
        {
            ShowTeachingTip("输入为空，请重新输入");
        }
        else
        {
            var result = await WebApi.TryRefreshCookie(newCookie);

            //Cookie有用
            ShowTeachingTip(result ? "Cookie有效，已保存" : "Cookie无效，请重新输入");
        }
    }

    private void ShowLoginWindow(object sender, RoutedEventArgs e)
    {
        LoginWindow.ShowLoginWindow(LoginCompleted);
    }
}