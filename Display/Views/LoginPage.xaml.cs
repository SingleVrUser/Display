using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private DispatcherTimer _qrTimer;
        //public QRCodeInfo QRCodeInfo;
        //public TokenInfo TokenInfo;
        public HttpClient Client;
        //public UserInfo UserInfo;
        ObservableCollection<Datum> groups;
        ObservableCollection<string> failList;
        private WebApi webapi = new();

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            groups = new ObservableCollection<Datum>();
            failList = new ObservableCollection<string>();
            BaseExample.ItemsSource = groups;
            FaileListView.ItemsSource = failList;

            //检查登录状态
            ResizeHttpClient();

            //加载数据
            loadData();
        }

        private async void loadData()
        {
            //List<Datum> datumList = DataAccess.LoadDataAccess();

            List<Datum> datumList = await Task.Run(() => DataAccess.LoadDataAccess());

            string[] videotype = { "mp4", "wmv", "iso", "avi", "mkv", "rmvb", "ts" };

            List<string> videoList = new List<string>();
            List<string> cidList = new List<string>();

            //暂时不显示了
            //foreach (var datum in datumList)
            //{
            //    groups.Add(datum);


            //    var FileName = datum.n;
            //    //挑选视频文件
            //    var extension = Path.GetExtension(FileName).Replace(".", "");

            //    //无后缀名文件、文件夹，跳过
            //    if (extension == "") continue;
            //    extension = extension.ToLower();

            //    //视频文件
            //    if (videotype.Contains(extension))
            //    {
            //        videoList.Add(FileName);

            //        // 正则匹配番号
            //        var VideoName = FileMatch.MatchName(FileName);
            //        if (VideoName == null) continue;

            //        if (!cidList.Contains(VideoName))
            //        {
            //            cidList.Add(VideoName);
            //        }
            //        else
            //        {
            //            failList.Add(FileName);
            //        }
            //    }
            //}

            //ResultBox.Text = $"总文件数量：{(groups.Count)}";
            //FileTexttBox.Text = $"视频文件数量： {(videoList.Count)}";
            //videoTexttBox.Text = $"匹配的视频数量： {(cidList.Count)}";

            ResultBox.Text = "0";

        }

        /// <summary>
        /// 初始化网络状态
        /// </summary>
        private async void ResizeHttpClient()
        {
            QRLoadingRing.IsActive = true;

            Client = webapi.Client;

            if (WebApi.UserInfo != null || await webapi.UpdateLoginInfo())
            {
                LoginButton.IsEnabled = false;
                LoginButton.Content = "已登录";
                ImportButton.Visibility = Visibility.Visible;

                //显示头像
                PersonPicture.ProfilePicture = new BitmapImage(new Uri(WebApi.UserInfo.data.face));

                HiddenToggleSwitch.IsOn = await webapi.IsHiddenModel();
                HidenStackPanel.Visibility = Visibility.Visible;
            }

            QRLoadingRing.IsActive = false;
        }

        /// <summary>
        /// 展示二维码
        /// </summary>
        /// <param name="Text"></param>
        private async void QRCodeShow(string Text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(Text, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

            //e.g. Windows 10 Universal App (UAP)
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeAsPngByteArr);
                    await writer.StoreAsync();
                }

                //显示二维码
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);

                QRLoadingRing.IsActive = false;
                QRCodeImage.Source = image;
            }

            StartQRLoginListener();
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
        /// 开始监听任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnQRTimerTickAsync(object sender, object e)
        {
            var result = await webapi.NetworkVerifyTokenAsync();

            if (result.state == 1)
            {
                LoginButton.Content = webapi.TokenInfo.data.user_name;
                StopQRLoginListener();
                return;
            }


            //if (await NetworkVerifyTokenAsync())
            //{
            //}
        }

        /// <summary>
        /// 暂停监听是否登录
        /// </summary>
        internal async void StopQRLoginListener()
        {
            _qrTimer?.Stop();
            _qrTimer = null;

            QRCodeImage.Source = null;

            LoginButton.IsEnabled = false;

            if (webapi.TokenInfo != null)
            {
                QRLoadingRing.IsActive = true;
                QRCodeImage.Source = new BitmapImage(new Uri(webapi.TokenInfo.data.face.face_l));
                HiddenToggleSwitch.IsOn = await webapi.IsHiddenModel();
                HidenStackPanel.Visibility = Visibility.Visible;
                QRLoadingRing.IsActive = false;
            }
        }

        /// <summary>
        /// 获取所有文件信息
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="webFileInfoList"></param>
        /// <returns></returns>
        private async Task<List<Datum>> GetAllFileInfo(string cid = "0", List<Datum> webFileInfoList = null)
        {
            if (webFileInfoList == null)
            {
                webFileInfoList = new List<Datum>();
            }
            var limit = 40;
            var WebFileInfo = webapi.GetFile(cid, limit);

            if (WebFileInfo.errNo == 20130827)
            {
                WebFileInfo = webapi.GetFile(cid);
            }

            //实际count 大于设置的limit
            if (WebFileInfo.count > limit)
            {
                limit = WebFileInfo.count;
                WebFileInfo = webapi.GetFile(cid, limit);
            }

            if (WebFileInfo.state)
            {
                foreach (var item in WebFileInfo.data)
                {
                    groups.Add(item);
                    ResultBox.Text = (Int32.Parse(ResultBox.Text) + 1).ToString();
                    webFileInfoList.Add(item);

                    DataAccess.AddFilesInfo(item);


                    //文件夹
                    if (item.fid == null)
                    {
                        webFileInfoList = await GetAllFileInfo(item.cid, webFileInfoList);
                    }

                }

            }

            return webFileInfoList;
        }

        /// <summary>
        /// 登录按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            QRLoadingRing.IsActive = true;

            //var response = await Client.GetAsync("https://qrcodeapi.115.com/api/1.0/web/1.0/token");
            //var result = await response.Content.ReadAsStringAsync();
            //webapi.QRCodeInfo = JsonConvert.DeserializeObject<QRCodeInfo>(result);

            QRCodeInfo QRCodeInfo = await webapi.GetQRCodeInfo();
            LoginButton.Content = QRCodeInfo.data.qrcode;
            QRCodeShow(QRCodeInfo.data.qrcode);
        }

        /// <summary>
        /// 按下开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var AllFileInfo = await GetAllFileInfo();

            ResultBox.Text = $"共获取到{AllFileInfo.Count}个数据";
        }

    }
}
