using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using static Display.App;
using Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Http;
using HtmlAgilityPack;
using System.IO;
using System.Net;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportPage : Page
    {
        ObservableCollection<Datum> Items =
            new ObservableCollection<Datum>();
        ObservableCollection<Datum> SelectedItems =
            new ObservableCollection<Datum>();
        ObservableCollection<string> FailItems =
            new ObservableCollection<string>();
        ObservableCollection<Datum> Breadcrumbs =
            new ObservableCollection<Datum>();
        //ObservableCollection<string> ResultInfoItems =
        //    new ObservableCollection<string>();
        FileProgress fileProgress = new FileProgress();
        string JavBusUrl = "https://www.busjav.fun";
        string JavDBUrl = "https://javdb.com";
        string SavePath = "D:/库/Pictures/应用缓存";
        public VideoInfo resultinfo = new VideoInfo();

        public ImportPage()
        {
            this.InitializeComponent();

            InitializeView();
        }

        private void InitializeView()
        {
            var data = DataAccess.GetFolderListByPid("0");

            Items.Clear();
            foreach (var item in data)
            {
                Items.Add(item);
            }

            FolderListView.ItemsSource = Items;
            SelectedListView.ItemsSource = SelectedItems;
            FailListView.ItemsSource = FailItems;

            if (data.Count == 0) return;

            //测试
            //SelectedItems.Add(data[0]);

            Breadcrumbs.Clear();
            Breadcrumbs.Add(new Datum() { n = "Home", cid = "0" });
        }


        private void FolderBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            // Don't process last index (current location)
            if (args.Index < Breadcrumbs.Count - 1)
            {
                // Home is special case.
                if (args.Index == 0)
                {
                    InitializeView();
                }
                // Go back to the clicked item.
                else
                {
                    var item = (Datum)args.Item;
                    var data = DataAccess.GetListByCid(item.cid);
                    Items.Clear();
                    foreach (var file_info in data)
                    {
                        Items.Add(file_info);
                    }

                    // Remove breadcrumbs at the end until 
                    // you get to the one that was clicked.
                    while (Breadcrumbs.Count > args.Index + 1)
                    {
                        Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);

                    }
                }
            }
        }


        private void FolderListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Datum;
            //选择
            if ((bool)FolderToggleButton.IsChecked)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
            //文件模式
            else
            {
                //选中文件夹，则跳转
                if (item.fid is "")
                {
                    Breadcrumbs.Add(new Datum() { n = item.n, cid = item.cid });
                    var data = DataAccess.GetListByCid(item.cid);
                    Items.Clear();
                    foreach (var file_info in data)
                    {
                        Items.Add(file_info);
                    }
                }
            }
        }

        /// <summary>
        /// 从文件中挑选出视频文件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<string> GetVideoAndMatchFile(List<Datum> data)
        {
            //根据视频信息匹配视频文件
            List<string> cidList = new List<string>();

            foreach (var file_info in data)
            {
                string FileName = file_info.n;

                //挑选视频文件
                if (file_info.vdi != 0)
                {
                    //根据视频名称匹配番号
                    var VideoName = FileMatch.MatchName(FileName);
                    if (VideoName == null) continue;

                    if (!cidList.Contains(VideoName))
                    {
                        cidList.Add(VideoName);
                    }
                }
            }

            return cidList;
        }

        private async Task SearchVideoInfo()
        {
            // 正则匹配的视频文件
            var cidList = new List<string>();

            StartProgressBar.Visibility = Visibility.Visible;

            foreach (var item in SelectedItems)
            {
                List<Datum> data = await Task.Run(() => GetAllFile(item.cid));

                cidList = cidList.Concat(await Task.Run(() => GetVideoAndMatchFile(data))).ToList<string>();
            }

            StartProgressBar.Visibility = Visibility.Collapsed;
            string StartText = $"正则匹配视频的数量：{cidList.Count}";
            FailTextBlock.Text = StartText;

            //初始化进度条
            Doing_ProgressBar.Maximum = cidList.Count;
            Doing_ProgressBar.Value = 0;

            foreach (var Cid in cidList)
            {
                //进度条
                Doing_ProgressBar.Value += 1;
                //进度信息
                DoingInfo_TextBlock.Text = Cid;
                Result_TextBlock.Text = $"[{Doing_ProgressBar.Value}/{cidList.Count}]";

                var WebFileInfo = await SearchInfo(Cid);

                // 搜索信息无果
                if (WebFileInfo == null)
                {
                    //failList.Add(Cid);
                    FailItems.Add(Cid);

                    FailTextBlock.Text = StartText + $"\n失败总数：{FailItems.Count}";
                }
                else
                {
                    UpdateVideoInfoDisplay(WebFileInfo);
                }

                // 初始化进度条
                SearchWayTextBlock.Text = null;
                SearchWayHyperLinkButton.Content = null;
                SearchWayProgressRing.IsActive = false;
            }

        }

        /// <summary>
        /// 开始按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Click_FolderBreadcrumbBar(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await SearchVideoInfo();

            //完成
            Done_Button.Visibility = Visibility.Visible;

        }

        private void UpdateVideoInfoDisplay(Data.VideoInfo WebFileInfo)
        {
            //更新属性
            resultinfo.truename = WebFileInfo.truename;
            resultinfo.releasetime = WebFileInfo.releasetime;
            resultinfo.lengthtime = WebFileInfo.lengthtime;
            resultinfo.director = WebFileInfo.director;
            resultinfo.producer = WebFileInfo.producer;
            resultinfo.publisher = WebFileInfo.publisher;
            resultinfo.series = WebFileInfo.series;
            resultinfo.category = WebFileInfo.category;
            resultinfo.actor = WebFileInfo.actor;
            resultinfo.imagepath = WebFileInfo.imagepath;


            VideoInfo_HyperlinkButton.Content = WebFileInfo.truename;
            VideoInfo_HyperlinkButton.NavigateUri = new Uri(WebFileInfo.busurl);

            //显示图片
            BitmapImage bitmapImage = new();
            bitmapImage.UriSource = new Uri(Cover_Image.BaseUri, WebFileInfo.imagepath);
            Cover_Image.Source = bitmapImage;
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        private List<Datum> GetAllFile(string cid)
        {
            List<Datum> list = new List<Datum>();
            var data = DataAccess.GetListByCid(cid);

            foreach (var item in data)
            {
                //文件夹
                if (item.fid == "")
                {
                    var resultList = GetAllFile(item.cid);
                    foreach (Datum datum in resultList)
                    {
                        list.Add(datum);
                    }
                }
                else
                {
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// 等待startSecond到endSecond秒后继续，文本控件showText提示正在倒计时
        /// </summary>
        /// <param name="startSecond"></param>
        /// <param name="endSecond"></param>
        /// <param name="showText"></param>
        private async Task RandomCoutDown(int startSecond, int endSecond, TextBlock showText = null)
        {
            //随机等待1-10s
            int randomSecond = new Random().Next(startSecond, endSecond);

            //倒计时
            for (int i = 0; i < randomSecond; i++)
            {
                if (showText != null)
                {
                    showText.Text = $"随机等待，{randomSecond - i}s后开始";
                    await Task.Delay(1000);
                }
            }
            if (showText != null)
            {
                showText.Text = null;
            };

        }

        private async Task<VideoInfo> SearchInfo(string VideoName)
        {
            VideoInfo tmpFile = new();

            var CID = VideoName;

            //如果数据库已存在该数据
            var result = DataAccess.SelectTrueName(CID.ToUpper());
            if (result.Count != 0)
            {
                SearchWayTextBlock.Text = "本地检索";
                SearchWayHyperLinkButton.Content = "本地数据库";
                SearchWayHyperLinkButton.IsEnabled = false;
                tmpFile = await Task.Run(() => DataAccess.LoadOneVideoInfoByCID(result[0]));
            }
            // 从相关网站中搜索
            else
            {
                // 随机等待1-2秒
                await RandomCoutDown(1, 2, SearchWayTextBlock);

                //先从javbus中搜索
                tmpFile = await SearchInfoFromJavBus(CID);

                //搜索无果，使用javdb搜索
                if (tmpFile == null)
                {
                    tmpFile = await SearchInfoFromJavDB(CID);
                }

                //多次搜索无果，退出
                if (tmpFile == null) return null;

                // 添加进数据库
                DataAccess.AddVideoInfo(tmpFile);
            }
            return tmpFile;
        }


        /// <summary>
        /// 从javbus中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private async Task<VideoInfo> SearchInfoFromJavBus(string CID)
        {
            VideoInfo videoInfo = new VideoInfo();

            HttpClient Client = new HttpClient();
            string busurl = $"{JavBusUrl}/{CID}";

            Uri uri = new Uri(busurl);
            videoInfo.busurl = busurl;


            SearchWayTextBlock.Text = "从JavBus中搜索";
            SearchWayHyperLinkButton.Content = busurl;
            SearchWayHyperLinkButton.NavigateUri = uri;
            SearchWayHyperLinkButton.IsEnabled = true;
            SearchWayProgressRing.IsActive = true;


            // 访问
            HttpResponseMessage response;
            string strResult;
            try
            {
                response = await Client.GetAsync(uri);
                strResult = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            var ImageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9 screencap']//a//img");
            //搜索无果，退出
            if (ImageUrlNode == null) return null;

            var ImageUrl = ImageUrlNode.Attributes["src"].Value;
            if (!ImageUrl.Contains("http"))
            {
                ImageUrl = $"{JavBusUrl}{ImageUrl}";
            }

            ////下载封面
            var AttributeNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']//p");
            string filePath = $"{SavePath}/{CID}";
            videoInfo.imagepath = await downloadImage(ImageUrl, filePath, CID);

            //其他信息
            for (var i = 0; i < AttributeNodes.Count; i++)
            {
                var AttributeNode = AttributeNodes[i];

                var header = AttributeNode.FirstChild.InnerText.Trim();

                if (header == "識別碼:")
                {
                    videoInfo.truename = AttributeNode.SelectNodes(".//span")[1].InnerText.Trim();
                    VideoInfo_HyperlinkButton.Content = videoInfo.truename;
                    VideoInfo_HyperlinkButton.NavigateUri = uri;
                }
                else if (header == "發行日期:")
                {
                    videoInfo.releasetime = AttributeNode.LastChild.InnerText.Trim();
                }
                else if (header == "長度:")
                {
                    videoInfo.lengthtime = AttributeNode.LastChild.InnerText.Trim();
                }
                else if (header == "導演:")
                {
                    videoInfo.director = AttributeNode.LastChild.InnerText.Trim();
                }
                else if (header == "製作商:")
                {
                    videoInfo.producer = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
                }
                else if (header == "發行商:")
                {
                    videoInfo.publisher = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
                }
                else if (header == "系列:")
                {
                    videoInfo.series = AttributeNode.SelectSingleNode(".//a").InnerText.Trim();
                }
                else if (header == "類別:")
                {
                    var categoryNodes = AttributeNodes[i + 1].SelectNodes(".//span/label");
                    List<string> categoryList = new List<string>();
                    foreach (var node in categoryNodes)
                    {
                        categoryList.Add(node.InnerText);
                    }
                    videoInfo.category = string.Join(",", categoryList);
                }
                else if (header == "演員")
                {
                    if (i >= AttributeNodes.Count - 1) continue;
                    var actorNodes = AttributeNodes[i + 1].SelectNodes(".//span/a");
                    List<string> actorList = new();
                    foreach (var node in actorNodes)
                    {
                        actorList.Add(node.InnerText);
                    }
                    videoInfo.actor = string.Join(",", actorList);
                }
            }


            return videoInfo;
        }

        /// <summary>
        /// 从众多搜索结果中搜索出符合条件的
        /// </summary>
        /// <returns></returns>
        private string GetDetailUrlFromSearchResult(HtmlDocument htmlDoc, string CID)
        {
            string result = null;


            var SearchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'movie-list')]");


            //搜索无果，退出
            if (SearchResultNodes == null) return null;

            string left_cid = null;
            string right_cid = null;

            var split_result = CID.Split(new char[] { '-', '_' });
            if (CID.Contains("HEYDOUGA"))
            {
                left_cid = split_result[1];
                right_cid = split_result[2];
            }
            else
            {
                left_cid = split_result[0];
                right_cid = split_result[1];
            }

            string search_left_cid = null;
            string search_right_cid = null;
            for (var i = 0; i < SearchResultNodes.Count; i++)
            {
                var movie_list = SearchResultNodes[i];
                var title_search = movie_list.SelectSingleNode(".//div[@class='video-title']/strong").InnerText;
                string title = title_search.ToUpper();

                split_result = title.Split(new char[] { '-', '_' });
                if (split_result.Length == 1)
                {
                    var match_result = Regex.Match(title, @"([a-zA-Z]+)(\d+)");
                    if (match_result == null) continue;
                    search_left_cid = match_result.Groups[1].Value;
                    search_right_cid = match_result.Groups[2].Value;
                }
                else if (split_result.Length == 2)
                {
                    search_left_cid = split_result[0];
                    search_right_cid = split_result[1];
                }
                else if (split_result.Length == 3)
                {
                    if (title.Contains("HEYDOUGA"))
                    {
                        search_left_cid = split_result[1];
                        search_right_cid = split_result[2];
                    }

                }
                if (search_left_cid == left_cid && search_right_cid == right_cid)
                {
                    var detail_url = SearchResultNodes[i].SelectSingleNode(".//a").Attributes["href"].Value;
                    detail_url = $"{JavDBUrl}{detail_url}";
                    result = detail_url;
                    break;
                }

            }

            return result;
        }

        private async Task<string> GetDetailUrlFromJavDBSearchResult(string CID)
        {
            string result = null;

            //反爬严重，尝试添加user-agent
            HttpClient Client = new HttpClient();
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");


            string busurl = $"{JavDBUrl}/search?q={CID}&f=all";
            Uri uri = new Uri(busurl);


            SearchWayTextBlock.Text = "使用关键词从JavDB中搜索";
            SearchWayHyperLinkButton.Content = uri;
            SearchWayHyperLinkButton.NavigateUri = uri;
            SearchWayHyperLinkButton.IsEnabled = true;
            SearchWayProgressRing.IsActive = true;

            // 访问
            HttpResponseMessage response = null;
            string strResult = null;
            try
            {
                response = await Client.GetAsync(uri);
                strResult = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
            //var strResult = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            //添加此信息会卡住UI
            //FailItems.Add(strResult.Length.ToString());
            //FailItems.Add(strResult.Replace("\n", ""));

            result = GetDetailUrlFromSearchResult(htmlDoc, CID);

            return result;
        }

        /// <summary>
        /// 从javdb中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private async Task<VideoInfo> SearchInfoFromJavDB(string CID)
        {

            CID = CID.ToUpper();

            var detail_url = await GetDetailUrlFromJavDBSearchResult(CID);
            //搜索无果，退出
            if (detail_url == null) return null;

            // 初始化进度条
            SearchWayTextBlock.Text = null;
            SearchWayHyperLinkButton.Content = null;
            SearchWayProgressRing.IsActive = false;

            //两次访问间隔不能太短
            // 随机等待1-10秒
            await RandomCoutDown(3, 10, SearchWayTextBlock);

            HttpClient Client = new HttpClient();
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");

            //访问fc内容需要cookie
            if (CID.Contains("FC"))
            {
                Client.DefaultRequestHeaders.Add("cookie", "list_mode=h; theme=auto; over18=1; locale=zh; remember_me_token=eyJfcmFpbHMiOnsibWVzc2FnZSI6IklrUkllRGxGYVdKUmVVTjFNMmxFU21SelUzUk5JZz09IiwiZXhwIjoiMjAyMi0wNi0xN1QwODozMDoyNC4wMDBaIiwicHVyIjoiY29va2llLnJlbWVtYmVyX21lX3Rva2VuIn19--414e77cd762f36a3387d49cb682e070e27bd912a; __cf_bm=sSqOzEy2tsPQIxlDotEVv.42WoikPNiGLBZdkkRAQb0-1654926901-0-AWkKbaAn/qGu+isRTLeQn7rx8St85W2ywT2xDX5KsqHe38K/B5rRjVlWkvLUHkmIQlZBo7edG8tJIGV7wvXXoJEQDdIbCize6C6sKzvn+ILlYHV6ObJyc6e2OxV/5kVjhg==; _jdb_session=QhSQbmFYUmhiivYkt1U24v7TqpgoSvDhIRJ3HyGLQi7pERoZk6evWzfqXNPxnGVngIADRUw33T0JwAWhy5fCzwGqkr4fCE25oS29uwvgMAFlStUx8fbNLZvpLzKjClpLEImXCI6vbIx7O2dnLGRF0XxUnsTjVZW4Uzr6OZDbceds%2B3LUOLUJMQShYspYfNfeJZyjz%2BAfLsiQ3HX%2Bi87cnVK%2Ba5Xz2eBBg3fWZ7eCnwbHZNzQzlZLse%2FSnRivJTnRpdgrSo6v8%2F0%2Fn1Kd1d5GWdDGLYgZEY3ZhSrN1CflMpN42K026UPRL9nfN1XNofHDO1ZlgelNSXbyLSdEbI9Q5CFWRr9iLdGkcoSyNgwgPKTJGGySf7aVgnT7maaaOQ%3D%3D--yeSmCr0mKDRHYKqu--8oK%2FuVKolerm0seYWxCxKA%3D%3D");
            }

            Uri uri = new Uri(detail_url);

            SearchWayTextBlock.Text = "从JavDB中搜索";
            SearchWayHyperLinkButton.Content = uri;
            SearchWayHyperLinkButton.NavigateUri = uri;
            SearchWayHyperLinkButton.IsEnabled = true;
            SearchWayProgressRing.IsActive = false;

            // 访问
            HttpResponseMessage response = null;
            string strResult = null;
            try
            {
                response = await Client.GetAsync(uri);
                strResult = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }

            ////查看是否触发反爬虫机制
            //if (strResult.Count() < 200)
            //{
            //    return null;
            //}

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            var video_meta_panelNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-meta-panel']");
            //搜索无果，退出
            if (video_meta_panelNode == null) return null;

            VideoInfo videoInfo = new VideoInfo();
            videoInfo.busurl = detail_url;


            var ImageUrl = video_meta_panelNode.SelectSingleNode(".//img[@class='video-cover']").Attributes["src"].Value;
            if (!ImageUrl.Contains("http"))
            {
                ImageUrl = $"{JavDBUrl}{ImageUrl}";
            }


            ////下载封面
            string filePath = $"{SavePath}/{CID}";
            videoInfo.imagepath = await downloadImage(ImageUrl, filePath, CID);

            var AttributeNodes = video_meta_panelNode.SelectNodes(".//div[contains(@class,'panel-block')]");
            //其他信息
            for (var i = 0; i < AttributeNodes.Count; i++)
            {
                var keyNode = AttributeNodes[i].SelectSingleNode("strong");
                if (keyNode == null) continue;
                string key = keyNode.InnerText;

                var valueNode = AttributeNodes[i].SelectSingleNode("span");

                if (key.Contains("番號"))
                {
                    videoInfo.truename = valueNode.InnerText;
                }
                else if (key.Contains("日期"))
                {
                    videoInfo.releasetime = valueNode.InnerText;
                }
                else if (key.Contains("時長"))
                {
                    videoInfo.lengthtime = valueNode.InnerText;
                }
                else if (key.Contains("片商") || key.Contains("賣家"))
                {
                    videoInfo.producer = valueNode.InnerText;
                }
                else if (key.Contains("發行"))
                {
                    videoInfo.publisher = valueNode.InnerText;
                }
                else if (key.Contains("系列"))
                {
                    videoInfo.series = valueNode.InnerText;
                }
                else if (key.Contains("類別"))
                {
                    var categoryNodes = valueNode.SelectNodes("a");
                    List<string> categoryList = new List<string>();
                    foreach (var node in categoryNodes)
                    {
                        categoryList.Add(node.InnerText);
                    }
                    videoInfo.category = string.Join(",", categoryList);
                }
                else if (key.Contains("演員"))
                {
                    var actorNodes = valueNode.SelectNodes("a");
                    if (actorNodes == null) continue;
                    List<string> actorList = new List<string>();
                    foreach (var node in actorNodes)
                    {
                        actorList.Add(node.InnerText);
                    }
                    videoInfo.actor = string.Join(",", actorList);
                }
            }


            return videoInfo;
        }


        /// <summary>
        /// 下载图片，并返回图片路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> downloadImage(string url, string filePath, string fileName)
        {
            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string localFilename = $"{fileName}{Path.GetExtension(url)}";
            string localPath = Path.Combine(filePath, localFilename);

            if (!File.Exists(localPath))
            {
                using var httpClient = new HttpClient();
                //Uri uri = new Uri(url);
                byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
                //string documentsPath = System.Environment.GetFolderPath(
                //        System.Environment.SpecialFolder.Personal);

                File.WriteAllBytes(localPath, imageBytes);
            }

            return localPath;

        }


        //private async void Download_Click(object sender, RoutedEventArgs e)
        //{
        //    await downloadImage("https://www.busjav.fun/pics/cover/8ud6_b.jpg", "D:/库/Pictures/应用缓存","test");
        //}
    }

    public class FileProgress : INotifyPropertyChanged
    {
        public string Filename { get; set; }
        public string VideoInfo { get; set; }
        public string ReleaseTime { get; set; }
        public string LengthTime { get; set; }
        public string Director { get; set; }
        public string Producers { get; set; }
        public string Publisher { get; set; }
        public string Series { get; set; }
        public string Category { get; set; }
        public string Actor { get; set; }

        private Visibility _buttonVisibility = Visibility.Collapsed;
        public Visibility ButtonVisibility
        {
            get { return _buttonVisibility; }
            set
            {
                _buttonVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _progressVisibility = Visibility.Collapsed;
        public Visibility progressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                RaisePropertyChanged();
            }
        }

        private int _progressValue;
        public int progressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }


}
