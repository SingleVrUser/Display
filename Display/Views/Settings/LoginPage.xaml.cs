
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Display.CustomWindows;
using Display.Models.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private Window window;
        private WebApi webapi;
        private DispatcherTimer _qrTimer;
        private bool _waitfresh = false;

        public LoginPage()
        {
            this.InitializeComponent();

            //appWindow = App.getAppWindow(this);
            window = LoginWindow.m_window;

            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(AppTitleBar);

            webapi = WebApi.GlobalWebApi;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            tryLogin();
        }

        private async void tryLogin()
        {
            //Task.Run(() => webapi.GetQRCodeInfo());


            QRCodeInfo qrcodeInfo = await webapi.GetQrCodeInfo();
            if (ImageGrid.Visibility == Visibility.Visible)
            {
                ImageGrid.Visibility = Visibility.Collapsed;
            }

            //显示二维码
            QRCodeShow(qrcodeInfo.data.qrcode);

            //状态：登录中
            UpdateState("login");

            //检查二维码登录状态
            var isLogined = false;
            var isError = false;
            for (int i = 0; i < 10; i++)
            {
                //登录成功或发生错误
                if (isLogined || isError)
                {
                    break;
                }
                //二维码已更新
                else if (qrcodeInfo.data.uid != WebApi.QrCodeInfo.data.uid)
                {
                    return;
                }

                //发送查询请求，可能为超长时长（服务器单次最大通讯时长30s）
                var qRCodeStatus = await webapi.GetQrCodeStatusAsync();

                if (qRCodeStatus.state == 1)
                {
                    var statusInfo = qRCodeStatus.data;
                    switch (statusInfo.status)
                    {
                        case 1:
                            UpdateState("wait", statusInfo.msg, "info");
                            break;
                        case 2:
                            UpdateState("wait", "登录成功", "info");
                            isLogined = true;
                            break;
                        case -2:
                            UpdateState("wait", statusInfo.msg, "fresh");
                            isError = true;
                            break;
                        case -1:
                            UpdateState("error", statusInfo.msg, "fresh");
                            isError = true;
                            break;
                    }
                }
                //二维码过期，超过5min
                else
                {
                    UpdateState("error", qRCodeStatus.message, "error");
                    break;
                }
            }

            //成功登录后，检查网络状态（内含存储Cookie），关闭窗口
            if (isLogined)
            {
                await webapi.NetworkVerifyTokenAsync();
                await showSuccessAndExite();
            }
        }

        private async Task showSuccessAndExite()
        {
            //显示成功提示
            SuccessTip.IsOpen = true;

            //等待2秒后退出
            await Task.Delay(2000);
            window.Close();
        }

        /// <summary>
        /// 展示二维码
        /// </summary>
        /// <param Name="Text"></param>
        private async void QRCodeShow(string Text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(Text, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

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

            //QRLoadingRing.IsActive = false;
            QrCodeImage.Source = image;
        }

        /// <summary>
        /// 开始监听二维码扫描状态
        /// </summary>
        internal void StartQRLoginListener()
        {
            if (_qrTimer == null)
            {
                _qrTimer = new DispatcherTimer();
                _qrTimer.Interval = TimeSpan.FromSeconds(3);
                _qrTimer.Tick += OnQRTimerTickAsync;
            }

            _qrTimer?.Start();
        }

        /// <summary>
        /// 监听二维码请求结果
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private async void OnQRTimerTickAsync(object sender, object e)
        {
            var result = await webapi.NetworkVerifyTokenAsync();

            //成功登录
            if (result.state == 1)
            {
                StopQRLoginListener();
                return;
            }
            //发生错误
            else if (result.error != null)
            {
                if (result.errno == 40101017)
                {
                    //显示错误信息
                    UpdateState("wait", result.message, "error");
                }
                else
                {
                    //停止监听
                    StopQRLoginListener();

                    //显示错误信息
                    UpdateState("errorOccurred", result.message, "error");
                }

            }


            //if (await NetworkVerifyTokenAsync())
            //{
            //}
        }

        /// <summary>
        /// 暂停监听是否登录
        /// </summary>
        internal void StopQRLoginListener()
        {
            _qrTimer?.Stop();
            _qrTimer = null;

            //QRCodeImage.Source = null;

            //LoginButton.IsEnabled = false;

            if (webapi.TokenInfo != null && webapi.TokenInfo.data != null)
            {
                //QRLoadingRing.IsActive = true;
                QrCodeImage.Source = new BitmapImage(new Uri(webapi.TokenInfo.data.face.face_l));
                //HiddenToggleSwitch.IsOn = await webapi.IsHiddenModel();
                //HidenStackPanel.Visibility = Visibility.Visible;
                //QRLoadingRing.IsActive = false;
            }
        }

        private void CheckCookieButton_Click(object sender, RoutedEventArgs e)
        {
            //termsOfUseContentDialog.Content = new ContentsPage.CheckCookie(CookieInputTextBox.Text);
            //await termsOfUseContentDialog.ShowAsync();
            CheckCookieTip.Content = new CheckCookie(CookieInputTextBox.Text);
            CheckCookieTip.IsOpen = true;

        }

        private void CookieInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CookieInputTextBox.Text != "" && CheckCookieButton.IsEnabled == false)
            {
                CheckCookieButton.IsEnabled = true;
            }
            else if (CookieInputTextBox.Text == "" && CheckCookieButton.IsEnabled == true)
            {
                CheckCookieButton.IsEnabled = false;
            }
        }

        private void UpdateState(string state = "login", string showName = "", string type = "info")
        {
            switch (state)
            {
                case "login":
                    tryShowQRCodeConfirmTip(Visibility.Collapsed);
                    updateSweepQRCodeTip();
                    break;
                case "wait" or "error":
                    tryShowQRCodeConfirmTip(Visibility.Visible, showName, type);
                    updateSweepQRCodeTip();
                    break;
            }
        }

        private void updateSweepQRCodeTip()
        {
            //QRCodeConfirmTip显示时，SweepQRCodeTip隐藏
            if (QrCodeConfirmTip.Visibility == Visibility.Visible && SweepQrCodeTip.Visibility == Visibility.Visible)
            {
                SweepQrCodeTip.Visibility = Visibility.Collapsed;
            }
            //QRCodeConfirmTip隐藏时，SweepQRCodeTip显示
            else if (QrCodeConfirmTip.Visibility == Visibility.Collapsed && SweepQrCodeTip.Visibility == Visibility.Collapsed)
            {
                SweepQrCodeTip.Visibility = Visibility.Visible;
            }
        }

        private void tryShowQRCodeConfirmTip(Visibility visibility, string showName = "", string type = "info")
        {
            if (showName == "")
            {
                showName = "扫描成功 请在手机点确认以登录";
            }

            //背景颜色
            switch (type)
            {
                case "info":
                    QrCodeConfirmTip.Background = new SolidColorBrush(Colors.LimeGreen);
                    break;
                //要求刷新
                case "fresh":
                    _waitfresh = true;
                    RefreshQrCodeGrid.Opacity = 1;
                    QrCodeConfirmTip.Background = new SolidColorBrush(Colors.SandyBrown);
                    break;
                case "error":
                    QrCodeConfirmTip.Background = new SolidColorBrush(Colors.OrangeRed);
                    break;
            }

            (QrCodeConfirmTip.Children.Where(x => x is TextBlock).FirstOrDefault() as TextBlock).Text = showName;

            //显示
            if (visibility == Visibility.Visible && QrCodeConfirmTip.Visibility == Visibility.Collapsed)
            {
                QrCodeConfirmTip.Visibility = Visibility.Visible;
            }
            //隐藏
            else if (visibility == Visibility.Collapsed && QrCodeConfirmTip.Visibility == Visibility.Visible)
            {
                QrCodeConfirmTip.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshQRCode_Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            RefreshQrCodeGrid.Opacity = 1;
            if (QrCodeImage.Source == null)
            {
                ImageGrid.Visibility = Visibility.Collapsed;
            }
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void RefreshQRCode_Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //等待二维码刷新就一直显示
            if (!_waitfresh)
            {
                RefreshQrCodeGrid.Opacity = 0;
            }
            if (QrCodeImage.Source == null)
            {
                ImageGrid.Visibility = Visibility.Visible;
            }
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void RefreshQRCode_Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _waitfresh = false;
            tryLogin();
        }

        private async void CookieInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (CookieInputTextBox.Text == "")
            {
                CookieInputTip.Subtitle = "输入为空，请重新输入";
                CookieInputTip.IsOpen = true;
            }
            else
            {
                var result = await webapi.TryRefreshCookie(CookieInputTextBox.Text);
                //Cookie有用
                if (result)
                {
                    await showSuccessAndExite();
                }
                else
                {
                    CookieInputTip.Subtitle = "Cookie无效，请重新输入";
                    CookieInputTip.IsOpen = true;
                }
            }
        }
    }
}
