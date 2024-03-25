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
    /// �����û���Ϣ
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
        //��ʾ��¼����
        var loginWindow = new LoginWindow();
        loginWindow.Activate();

        //�رյ�¼���ڣ�ˢ��ҳ��
        loginWindow.Closed += LoginWindow_Closed;
    }

    /// <summary>
    /// ��¼���ڹرպ�ˢ�µ�¼״̬
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
    /// ɾ��cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteCookieButton(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "ȷ��ɾ��",
            PrimaryButtonText = "ȷ��",
            CloseButtonText = "����",
            DefaultButton = ContentDialogButton.Close,
            Content = "���ȷ�Ϻ�ɾ��Ӧ���д洢��Cookie"
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        WebApi.DeleteCookie();
        CookieBox.Password = null;
    }


    /// <summary>
    /// ����cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CopyCookieButtonClick(object sender, RoutedEventArgs e)
    {
        //����һ�����ݰ�
        var dataPackage = new DataPackage();
        dataPackage.SetText(CookieBox.Password);

        //�����ݰ��ŵ���������
        Clipboard.SetContent(dataPackage);

        ShowTeachingTip("�Ѹ���");
    }

    /// <summary>
    /// ����Cookies
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ExportCookieButton(object sender, RoutedEventArgs e)
    {
        var cookies = CookieBox.Password;
        if (string.IsNullOrEmpty(cookies))
        {
            ShowTeachingTip("����Ϊ��");
            return;
        }

        var exportCookieList = FileMatch.ExportCookies(cookies);

        //���ô���������ı�����
        var clipboardText = System.Text.Json.JsonSerializer.Serialize(exportCookieList);

        //����һ�����ݰ�
        var dataPackage = new DataPackage();
        dataPackage.SetText(clipboardText);

        //�����ݰ��ŵ���������
        Clipboard.SetContent(dataPackage);

        var dataPackageView = Clipboard.GetContent();
        var text = await dataPackageView.GetTextAsync();
        if (text != clipboardText) return;

        ShowTeachingTip("����ӵ�������");
    }

    /// <summary>
    /// ��ʾ������Cookie
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

        ShowTeachingTip("�����");
    }


    #endregion

}