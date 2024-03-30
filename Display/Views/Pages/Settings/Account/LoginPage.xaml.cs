using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Display.Providers;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;

namespace Display.Views.Pages.Settings.Account;

public sealed partial class LoginPage
{
    private readonly WebApi _webapi = WebApi.GlobalWebApi;

    /// <summary>
    /// 是否正在等待二维码刷新
    /// </summary>
    private bool _waitingFresh;

    public Action LoginCompletedAction;

    public LoginPage() => InitializeComponent();

    private void Grid_Loaded(object sender, RoutedEventArgs e)
        => TryLogin();

    private async void TryLogin()
    {
        // 获取二维码图片信息
        var qrcodeInfo = await _webapi.GetQrCodeInfo();

        // 每次
        QrCodeWaitRefreshGrid.Visibility = Visibility.Collapsed;
        QrCodeMessageStackPanel.Visibility = Visibility.Collapsed;

        //显示二维码
        QrCodeShow(qrcodeInfo.data.Qrcode);

        //状态：登录中
        UpdateStateUI(QrCodeStatus.WaitScan);

        //监听二维码状态
        if (!await WaitLoginByQrCode(qrcodeInfo.data.Uid))
            return;

        //成功登录后，检查网络状态（内含存储Cookie），关闭窗口
        await _webapi.GetNetworkVerifyTokenAsync();
        await ShowSuccessAndExit();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <returns>是否成功登录，True表示成功登录</returns>
    private async Task<bool> WaitLoginByQrCode(string uid)
    {
        //检查二维码登录状态
        var breakFlag = false;

        for (var i = 0; i < 10; i++)
        {
            //登录成功或发生错误
            if (breakFlag) break;

            //二维码已更新
            if (uid != WebApi.QrCodeInfoResult.data.Uid) return false;

            //发送查询请求，可能为超长时长（服务器单次最大通讯时长30s）
            var qRCodeStatus = await _webapi.GetQrCodeStatusAsync();

            if (qRCodeStatus.State == 1)
            {
                var statusInfo = qRCodeStatus.Data;
                Debug.WriteLine($"当前状态码：{statusInfo.Status}，信息：{statusInfo.Msg}");

                switch (statusInfo.Status)
                {
                    case 0:
                        UpdateStateUI(QrCodeStatus.WaitScan, statusInfo.Msg);
                        break;
                    case 1:
                        UpdateStateUI(QrCodeStatus.WaitConfirm, statusInfo.Msg);
                        break;
                    case 2:
                        UpdateStateUI(QrCodeStatus.Success, "登录成功");
                        return true;
                    case -2:
                        UpdateStateUI(QrCodeStatus.NeedFresh, statusInfo.Msg);
                        breakFlag = true;
                        _waitingFresh = true;
                        break;
                    case -1:
                        UpdateStateUI(QrCodeStatus.Error, statusInfo.Msg);
                        breakFlag = true;
                        break;
                }
            }
            //二维码过期，超过5min
            else
            {
                UpdateStateUI(QrCodeStatus.Error, qRCodeStatus.Message);
                break;
            }
        }

        return false;
    }

    private async Task ShowSuccessAndExit()
    {
        //显示成功提示
        SuccessTip.IsOpen = true;

        //等待2秒后退出
        await Task.Delay(2000);
        LoginCompletedAction?.Invoke();
    }

    /// <summary>
    /// 展示二维码
    /// </summary>
    /// <param name="qrcode"></param>
    private async void QrCodeShow(string qrcode)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeAsPngByteArr = qrCode.GetGraphic(20);

        //e.g. Windows 10 Universal App (UAP)
        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(qrCodeAsPngByteArr);
            await writer.StoreAsync();
        }

        //显示二维码
        var image = new BitmapImage();
        await image.SetSourceAsync(stream);

        QrCodeImage.Source = image;
    }

    private void CheckCookieButton_Click(object sender, RoutedEventArgs e)
    {
        CheckCookieTip.Content = new CheckCookie(CookieInputTextBox.Text);
        CheckCookieTip.IsOpen = true;
    }

    private void UpdateStateUI(QrCodeStatus state, string message = null)
    {
        if (state == QrCodeStatus.WaitScan)
        {
            QrCodeScanTip.Visibility = Visibility.Visible;
            return;
        }

        QrCodeScanTip.Visibility = Visibility.Collapsed;

        if (string.IsNullOrEmpty(message))
        {
            QrCodeMessageStackPanel.Visibility = Visibility.Collapsed;
            return;
        }

        //背景颜色
        switch (state)
        {
            case QrCodeStatus.WaitConfirm:
                QrCodeMessageStackPanel.Background = new SolidColorBrush(Colors.LimeGreen);
                break;
            //要求刷新
            case QrCodeStatus.NeedFresh:
                QrCodeTapRefreshGrid.Opacity = 1;
                QrCodeMessageStackPanel.Background = new SolidColorBrush(Colors.SandyBrown);
                break;
            case QrCodeStatus.Error:
                QrCodeMessageStackPanel.Background = new SolidColorBrush(Colors.OrangeRed);
                break;
        }

        QrCodeMessageTextBlock.Text = message;
        QrCodeMessageStackPanel.Visibility = Visibility.Visible;
    }

    private void RefreshQRCode_Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        QrCodeTapRefreshGrid.Opacity = 1;

        if (QrCodeImage.Source == null) QrCodeWaitRefreshGrid.Visibility = Visibility.Collapsed;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void RefreshQRCode_Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        //等待二维码刷新就一直显示
        if (!_waitingFresh) QrCodeTapRefreshGrid.Opacity = 0;

        if (QrCodeImage.Source == null) QrCodeWaitRefreshGrid.Visibility = Visibility.Visible;

        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private void RefreshQRCode_Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        _waitingFresh = false;
        TryLogin();
    }

    private async void CookieInputButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CookieInputTextBox.Text))
        {
            CookieInputTip.Subtitle = "输入为空，请重新输入";
            CookieInputTip.IsOpen = true;
        }
        else
        {
            var result = await _webapi.TryRefreshCookie(CookieInputTextBox.Text);

            //Cookie有用
            if (result)
            {
                await ShowSuccessAndExit();
            }
            else
            {
                CookieInputTip.Subtitle = "Cookie无效，请重新输入";
                CookieInputTip.IsOpen = true;
            }
        }
    }

    private enum QrCodeStatus
    {
        // 等待扫描二维码
        WaitScan,
        // 扫描二维码后等待确认
        WaitConfirm,
        // 需要刷新
        NeedFresh,
        // 成功登录
        Success,
        // 发生错误
        Error
    }
}