using Data.Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Ocr;

namespace Data
{
    public class GetInfoFromNetwork
    {
        public static bool IsJavDbCookieVisiable = true;

        private static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = CreateClient(new() { { "user-agent", BrowserUserAgent } });
                }

                return _client;
            }
            set=> _client = value;
        }

        private static HttpClient ClientWithJavDBCookie;

        public static string BrowserUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 115Browser/8.3.0";
        public static string DesktopUserAgent = "Mozilla/5.0; Windows NT/10.0.19044; 115Desktop/2.0.1.7";

        public GetInfoFromNetwork()
        {
        }

        public static HttpClient CreateClient(Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return new HttpClient();
            }

            var handler = new HttpClientHandler { UseCookies = false };
            var Client = new HttpClient(handler);

            foreach (var header in headers)
            {
                Client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            return Client;
        }

        public static string UrlCombine(string uri1, string uri2)
        {

            Uri baseUri = new Uri(uri1);
            Uri myUri = new Uri(baseUri, uri2);
            return myUri.ToString();
        }

        /// <summary>
        /// 检查是否能访问该网页
        /// </summary>
        /// <param name="checkUrl"></param>
        /// <returns></returns>
        public async Task<bool> CheckUrlUseful(string checkUrl)
        {
            bool isUseful = true;
            HttpResponseMessage resp;
            try
            {
                resp = await Client.GetAsync(checkUrl);
            }
            catch (HttpRequestException)
            {
                isUseful = false;
            }

            return isUseful;
        }

        /// <summary>
        /// 等待startSecond到endSecond秒后继续，文本控件showText提示正在倒计时
        /// </summary>
        /// <param name="startSecond"></param>
        /// <param name="endSecond"></param>
        /// <param name="showText"></param>
        public static async Task RandomTimeDelay(int startSecond, int endSecond)
        {
            //随机等待1-10s
            int randomSecond = new Random().Next(startSecond, endSecond);

            //倒计时
            for (int i = 0; i < randomSecond; i++)
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 从LibreDmm中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromLibreDmm(string CID)
        {
            string BusUrl = AppSettings.LibreDmm_BaseUrl;
            string SavePath = AppSettings.Image_SavePath;

            CID = CID.ToUpper();
            string url = UrlCombine(BusUrl, $"movies/{CID}");

            string strResult = await RequestHelper.RequestHtml(Client, url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            //搜索封面
            string ImageUrl = null;
            var ImageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class='img-fluid']");
            if (ImageUrlNode != null)
            {
                ImageUrl = ImageUrlNode.Attributes["src"].Value;
            }

            VideoInfo videoInfo = new VideoInfo();
            videoInfo.busurl = url;
            videoInfo.truename = CID;

            //dmm肯定没有步兵
            videoInfo.is_wm = 0;

            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");
            if (titleNode != null)
                videoInfo.title = titleNode.InnerText.Trim();

            var keyNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-4']/dl/dt");
            if (keyNodes == null) return null;
            var valueNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-4']/dl/dd");
            //其他信息
            for (var i = 0; i < keyNodes.Count; i++)
            {
                var key = keyNodes[i].InnerText;
                switch (key)
                {
                    case "Release Date":
                        videoInfo.releasetime = valueNodes[i].InnerText.Trim();
                        break;
                    case "Directors":
                        videoInfo.director = valueNodes[i].InnerText.Trim();
                        break;
                    case "Genres":
                        var generesNodes = valueNodes[i].SelectNodes("ul/li");
                        videoInfo.category = string.Join(",", generesNodes.Select(x => x.InnerText.Trim()));
                        break;
                    case "Labels":
                        videoInfo.series = valueNodes[i].InnerText.Trim();
                        break;
                    case "Makers":
                        videoInfo.producer = valueNodes[i].InnerText.Trim();
                        break;
                    case "Volume":
                        videoInfo.lengthtime = valueNodes[i].InnerText.Trim().Replace(" minutes", "分钟");
                        break;
                }
            }

            //演员
            var actressesNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='card actress']");

            if (actressesNodes != null)
            {
                videoInfo.actor = string.Join(",", actressesNodes.Select(x => x.InnerText.Trim()));
            }

            //下载封面
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                string filePath = Path.Combine(SavePath, CID);
                videoInfo.imageurl = ImageUrl;
                videoInfo.imagepath = await downloadFile(ImageUrl, filePath, CID);
            }

            return videoInfo;
        }

        /// <summary>
        /// 从javbus中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromJavBus(string CID)
        {
            VideoInfo videoInfo = new VideoInfo();
            string JavBusUrl = AppSettings.JavBus_BaseUrl;
            string SavePath = AppSettings.Image_SavePath;

            CID = CID.ToUpper();

            var spliteCid = CID.Split("-");
            if (spliteCid.Count() != 2) return null;

            string searchCID;

            switch (spliteCid[0])
            {
                case "MIUM" or "MAAN":
                    searchCID = $"300{CID}";
                    break;
                case "JAC":
                    searchCID = $"390{CID}";
                    break;
                case "DSVR":
                    searchCID = $"3{CID}";
                    break;
                default:
                    searchCID = CID;
                    break;
            }


            string busurl = UrlCombine(JavBusUrl, searchCID);

            videoInfo.busurl = busurl;
            string strResult = await RequestHelper.RequestHtml(Client, busurl);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            //搜索封面
            var ImageUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9 screencap']//a//img");
            if (ImageUrlNode == null) return null;

            var ImageUrl = ImageUrlNode.Attributes["src"].Value;
            if (!ImageUrl.Contains("http"))
            {
                ImageUrl = UrlCombine(JavBusUrl, ImageUrl);
            }

            //标题
            var title = ImageUrlNode.Attributes["title"].Value;
            videoInfo.title = title;

            //是否步兵
            var activeNavbarNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='navbar']/ul[@class='nav navbar-nav']/li[@class='active']/a");
            if (activeNavbarNode != null)
            {
                switch(activeNavbarNode.InnerText)
                {
                    case "有碼":
                        videoInfo.is_wm = 0;
                        break;
                    case "無碼":
                        videoInfo.is_wm = 1;
                        break;
                }
            }
            var AttributeNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']//p");
            videoInfo.truename = CID;
            //信息
            for (var i = 0; i < AttributeNodes.Count; i++)
            {
                var AttributeNode = AttributeNodes[i];

                var header = AttributeNode.FirstChild.InnerText.Trim();

                if (header == "發行日期:")
                {
                    videoInfo.releasetime = AttributeNode.LastChild.InnerText.Trim();
                }
                else if (header == "長度:")
                {
                    videoInfo.lengthtime = AttributeNode.LastChild.InnerText.Trim().Replace("分鐘", "分钟");
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

            //下载封面
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                string filePath = Path.Combine(SavePath, CID);
                videoInfo.imageurl = ImageUrl;
                videoInfo.imagepath = await downloadFile(ImageUrl, filePath, CID);
            }

            var sampleBox_Nodes = htmlDoc.DocumentNode.SelectNodes("//a[@class='sample-box']");
            List<string> sampleUrlList = new();
            if (sampleBox_Nodes != null)
            {
                foreach (var node in sampleBox_Nodes)
                {
                    string sampleImageUrl = node.Attributes["href"].Value;
                    if (!sampleImageUrl.Contains("http"))
                    {
                        sampleImageUrl = UrlCombine(JavBusUrl, sampleImageUrl);
                    }
                    sampleUrlList.Add(sampleImageUrl);
                }
                videoInfo.sampleImageList = string.Join(",", sampleUrlList);
            }

            return videoInfo;
        }

        /// <summary>
        /// 从jav321中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromJav321(string CID)
        {
            VideoInfo videoInfo = new VideoInfo();
            string SearchUrl = UrlCombine(AppSettings.Jav321_BaseUrl,"search");

            CID = CID.ToUpper();

            var postValues = new Dictionary<string, string>
                {
                    { "sn", CID}
                };

            Tuple<string,string> result = await RequestHelper.PostHtml(Client, SearchUrl, postValues);
            if (result == null) return null;
            videoInfo.busurl= result.Item1;

            string strResult = result.Item2;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            //标题
            var TitleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-heading']/h3");
            if (TitleNode == null) return null;
            videoInfo.title = TitleNode.FirstChild.InnerText;

            //图片地址
            var ImageNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/div[@class='row'][2]/div[@class='col-md-3']/div//img[@class='img-responsive']");
            string ImageUrl = string.Empty;
            List<string> sampleUrlList = new List<string>();

            //没有样图
            if (ImageNodes == null)
            {
                var ImageNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-body'][1]/div[@class='row'][1]/div[@class='col-md-3'][1]/img[@class='img-responsive']");
                if(ImageNode== null) return null;

                string imgSrc = ImageNode.GetAttributeValue("src", string.Empty);

                ImageUrl = imgSrc;
                //if(!imgSrc.EndsWith("webp"))
                //    ImageUrl = imgSrc;
            }
            //有样图
            else
            {
                //第一张为封面
                //其余为缩略图
                for (int i = 0; i < ImageNodes.Count; i++)
                {
                    var imageNode = ImageNodes[i];
                    string imageUrl = imageNode.GetAttributeValue("src", string.Empty);

                    //if (imageUrl.EndsWith("webp")) continue;

                    if (i == 0)
                    {
                        ImageUrl = imageUrl;
                    }
                    else
                    {
                        sampleUrlList.Add(imageUrl);
                    }
                }
            }

            //CID
            videoInfo.truename = CID;

            //其他信息
            var InfoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='col-md-9']");

            //解析
            Dictionary<string, string> infos = new Dictionary<string, string>();
            string key = null;
            List<string> values = new List<string>();
            foreach (var info in InfoNode.ChildNodes)
            {
                string name = info.Name;

                string innerText = info.InnerText.Trim();

                //key(b)
                if (name == "b")
                {
                    //前一key已检索完成
                    if (values.Count != 0)
                    {
                        infos[key] = string.Join(",", values);

                        System.Diagnostics.Debug.WriteLine($"添加{infos[key]}");
                        values.Clear();
                    }

                    key = innerText;

                }
                //value（可跳转的a）
                else if (name == "a")
                {
                    values.Add(innerText);
                }
                //value（不可跳转的#text）
                else if (name == "#text" && innerText.Contains(":"))
                {
                    string value = innerText.Replace(":", string.Empty);

                    //每个演员名之间是用“&nbsp;”分割
                    if (value.Contains("&nbsp;"))
                        value = value.Replace(" &nbsp; ", ",").Replace(" &nbsp;", "");

                    if (!string.IsNullOrEmpty(value))
                        values.Add(value);
                }
            }
            //最后的添加
            infos[key] = string.Join(",", values);
            values.Clear();

            //添加进VideoInfo
            foreach (var info in infos)
            {
                switch (info.Key)
                {
                    case "出演者":
                        videoInfo.actor= info.Value.Trim();
                        break;
                    case "メーカー":
                        videoInfo.producer = info.Value.Trim();
                        break;
                    case "シリーズ":
                        videoInfo.series = info.Value.Trim();
                        break;
                    case "ジャンル":
                        videoInfo.category = info.Value.Trim();
                        break;
                    case "配信開始日":
                        videoInfo.releasetime = info.Value.Trim();
                        break;
                    case "収録時間":
                        videoInfo.lengthtime = info.Value.Trim().Replace(" minutes", "分钟");
                        break;
                }
            }

            if (sampleUrlList != null)
            {
                videoInfo.sampleImageList = string.Join(",", sampleUrlList);
            }

            ////下载封面
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                string SavePath = AppSettings.Image_SavePath;
                string filePath = Path.Combine(SavePath, CID);
                videoInfo.imageurl = ImageUrl;
                videoInfo.imagepath = await downloadFile(ImageUrl, filePath, CID);
            }
            //（接受无封面）

            return videoInfo;
        }

        /// <summary>
        /// 从Fc2Hub中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromFc2Hub(string CID)
        {
            VideoInfo videoInfo = new VideoInfo();

            string BaseUrl = AppSettings.Fc2hub_BaseUrl;
            string SavePath = AppSettings.Image_SavePath;

            string url = $"{BaseUrl}search?kw={CID.Replace("FC2-", "")}";

            videoInfo.busurl = url;

            //默认是步兵
            videoInfo.is_wm = 1;

            string strResult = await RequestHelper.RequestHtml(Client, url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            var jsons = htmlDoc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");

            if (jsons == null || jsons.Count == 0) return null;

            var jsonString = jsons.Last().InnerText;

            var json = JsonConvert.DeserializeObject<FcJson>(jsonString);

            if (json.name == null || json.image == null) return null;

            videoInfo.title = json.name;
            videoInfo.truename = CID;
            videoInfo.releasetime = json.datePublished.Replace("/", "-");
            //PTxHxMxS转x分钟
            videoInfo.lengthtime = Data.FileMatch.ConvertPtTimeToTotalMinute(json.duration);
            videoInfo.director = json.director;
            videoInfo.producer = "fc2";

            if (json.genre != null)
                videoInfo.category = string.Join(",", json.genre);

            if (json.actor != null)
                videoInfo.actor = string.Join(",", json.actor);


            string ImageUrl = string.Empty;
            if (json.image != null)
            {
                ImageUrl = json.image;
            }
            else
            {
                var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

                if (imageNode != null)
                {
                    ImageUrl = imageNode.GetAttributeValue("content", string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(ImageUrl))
            {
                ////下载封面
                string filePath = Path.Combine(SavePath, CID);
                videoInfo.imageurl = ImageUrl;
                videoInfo.imagepath = await downloadFile(ImageUrl, filePath, CID);
            }

            return videoInfo;
        }

        /// <summary>
        /// 从AvMoo中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromAvMoo(string CID)
        {
            CID = CID.ToUpper();

            var detail_url = await GetDetailUrlFromAvMooResult(CID);
            //搜索无果，退出
            if (detail_url == null) return null;

            string strResult = await RequestHelper.RequestHtml(Client, detail_url);
            if(string.IsNullOrEmpty(strResult)) return null;

            VideoInfo videoInfo = new VideoInfo();
            videoInfo.busurl = detail_url;
            videoInfo.truename = CID;

            //AvMoo肯定不是步兵
            videoInfo.is_wm = 0;

            videoInfo = await AnalysisInfoFromAvSoxOrAvMoo(strResult, videoInfo);

            //如果失败，videoInfo可能会变成null

            return videoInfo;
        }

        /// <summary>
        /// 向AvMoo发送搜索请求
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private async Task<string> GetDetailUrlFromAvMooResult(string CID)
        {
            string result;
            string url = UrlCombine(AppSettings.AvMoo_BaseUrl, $"cn/search/{CID}");

            // 访问
            string strResult = await RequestHelper.RequestHtml(Client, url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            result = GetDetailUrlFromAvMooSearchResult(htmlDoc, CID);

            return result;
        }

        /// <summary>
        /// 从AvMoo众多搜索结果中搜索出符合条件的
        /// </summary>
        /// <returns></returns>
        private string GetDetailUrlFromAvMooSearchResult(HtmlDocument htmlDoc, string CID)
        {
            string result = null;

            //是否提示搜索失败
            var alertNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'alert-danger')]");
            if (alertNode != null) return null;

            var SearchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='waterfall']/div[@class='item']");

            //搜索无果，退出
            if (SearchResultNodes == null) return null;

            //分割通过正则匹配得到的CID
            var spliteResult = SpliteLocalCID(CID);
            if (spliteResult == null) return null;

            string left_cid = spliteResult.Item1;
            string right_cid = spliteResult.Item2;

            string search_left_cid;
            string search_right_cid;
            for (var i = 0; i < SearchResultNodes.Count; i++)
            {
                var movie_list = SearchResultNodes[i];
                var title = movie_list.SelectSingleNode(".//div[@class='photo-info']/span/date").InnerText.ToUpper();

                var split_result = title.Split(new char[] { '-', '_' });
                if (split_result.Length == 1)
                {
                    var match_result = Regex.Match(title, @"([A-Z]+)(\d+)");
                    if (match_result == null) continue;
                    search_left_cid = match_result.Groups[1].Value;
                    search_right_cid = match_result.Groups[2].Value;
                }
                else if (split_result.Length == 2)
                {
                    search_left_cid = split_result[0];
                    search_right_cid = split_result[1];
                }
                else
                    continue;

                int currentNum;
                int searchNum;

                if (search_left_cid == left_cid
                         && (search_right_cid == right_cid
                                || (Int32.TryParse(right_cid, out currentNum)
                                        && Int32.TryParse(right_cid, out searchNum)
                                            && currentNum.Equals(searchNum))))
                {
                    var detail_url = SearchResultNodes[i].SelectSingleNode(".//a[@class='movie-box']").Attributes["href"].Value;

                    //只有“//”没有“http(s)://”
                    if (!detail_url.Contains("http") && detail_url.Contains("//"))
                    {
                        detail_url = $"https:{detail_url}";
                    }

                    result = detail_url;
                    break;
                }
                else
                    continue;
            }

            return result;
        }

        /// <summary>
        /// 解析html内容（只适合与AvSox和AvMoo），补充VideoInfo
        /// </summary>
        /// <param name="strResult"></param>
        /// <param name="videoInfo"></param>
        /// <returns></returns>
        private async Task<VideoInfo> AnalysisInfoFromAvSoxOrAvMoo(string strResult, VideoInfo videoInfo)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            string CID = videoInfo.truename;

            //封面图
            string CoverUrl = null;
            var ImageNode = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='bigImage']");
            if (ImageNode == null) return null;
            CoverUrl = ImageNode.Attributes["href"].Value;
            videoInfo.imageurl = CoverUrl;

            //标题（AvMoox在a标签上，AvSox在img标签上）
            var result = ImageNode.GetAttributeValue("title", string.Empty);
            if (!string.IsNullOrEmpty(result))
            {
                videoInfo.title = result;
            }
            else
            {
                var ImgNode = ImageNode.SelectSingleNode(".//img");

                if(ImgNode == null) return null;

                result = ImgNode.GetAttributeValue("title", string.Empty);

                if (string.IsNullOrEmpty(result))
                    return null;

                videoInfo.title = result;
            }

            //其他信息
            var InfoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='row movie']/div[@class='col-md-3 info']");

            //解析
            Dictionary<string, string> infos = new Dictionary<string, string>();
            string key = null;
            List<string> values = new List<string>();
            foreach (var info in InfoNode.ChildNodes)
            {
                string name = info.Name;

                if (name != "p") continue;

                if (info.HasAttributes)
                {
                    var className = info.GetAttributeValue("class", string.Empty);

                    string InnerText = info.InnerText.Replace(":", string.Empty).Trim();

                    if (className == "header")
                    {
                        //前一key已检索完成
                        if (values.Count != 0)
                        {
                            infos[key] = string.Join(",", values);

                            values.Clear();
                        }

                        key = InnerText;
                    }
                    else if (!string.IsNullOrEmpty(InnerText))
                    {
                        values.Add(InnerText);
                    }
                }
                else
                {
                    foreach (var child in info.ChildNodes)
                    {
                        var className = child.GetAttributeValue("class", string.Empty);

                        string childInnerText = child.InnerText.Replace(":", string.Empty).Trim();

                        if (className == "header")
                        {
                            //前一key已检索完成
                            if (values.Count != 0)
                            {
                                infos[key] = string.Join(",", values);

                                values.Clear();
                            }

                            key = childInnerText;
                        }
                        else if (!string.IsNullOrEmpty(childInnerText))
                        {
                            values.Add(childInnerText);
                        }
                    }

                }
            }
            //最后的添加
            infos[key] = string.Join(",", values);
            values.Clear();

            //添加进VideoInfo
            foreach (var info in infos)
            {
                switch (info.Key)
                {
                    case "发行时间":
                        videoInfo.releasetime = info.Value;
                        break;
                    case "长度":
                        videoInfo.lengthtime = info.Value;
                        break;
                    case "导演":
                        videoInfo.director = info.Value;
                        break;
                    case "制作商":
                        videoInfo.producer = info.Value;
                        break;
                    case "发行商":
                        videoInfo.publisher = info.Value;
                        break;
                    case "系列":
                        videoInfo.series = info.Value;
                        break;
                    case "类别":
                        videoInfo.category = info.Value;
                        break;
                }
            }

            //演员
            var ActorNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='avatar-waterfall']/a[@class='avatar-box']/span");
            if (ActorNodes != null)
                videoInfo.actor = string.Join(",", ActorNodes.Select(item => item.InnerText.Trim()).ToList());

            //样品图片
            List<string> sampleUrlList = new List<string>();
            var sampleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='sample-waterfall']/a[@class='sample-box']");

            if (sampleNodes != null)
            {
                foreach (var sampleNode in sampleNodes)
                {
                    sampleUrlList.Add(sampleNode.GetAttributeValue("href", string.Empty));
                }

                videoInfo.sampleImageList = string.Join(",", sampleUrlList);
            }

            //下载图片
            string filePath = Path.Combine(AppSettings.Image_SavePath, CID);
            videoInfo.imageurl = CoverUrl;
            videoInfo.imagepath = await downloadFile(CoverUrl, filePath, CID);

            return videoInfo;
        }

        /// <summary>
        /// 从AvSox中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromAvSox(string CID)
        {
            string SavePath = AppSettings.Image_SavePath;
            CID = CID.ToUpper();

            var detail_url = await GetDetailUrlFromAvSoxResult(CID);

            //搜索无果，退出
            if (detail_url == null) return null;

            string strResult = await RequestHelper.RequestHtml(Client, detail_url);
            if (string.IsNullOrEmpty(strResult)) return null;

            VideoInfo videoInfo = new VideoInfo();
            videoInfo.busurl = detail_url;
            videoInfo.truename = CID;

            //AvSox肯定是步兵
            videoInfo.is_wm = 1;

            videoInfo = await AnalysisInfoFromAvSoxOrAvMoo(strResult, videoInfo);

            //如果失败，videoInfo可能会变成null

            return videoInfo;
        }

        /// <summary>
        /// 向AvSox发送搜索请求
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private async Task<string> GetDetailUrlFromAvSoxResult(string CID)
        {
            string result;
            string url = UrlCombine(AppSettings.AvSox_BaseUrl, $"cn/search/{CID}");

            // 访问
            string strResult = await RequestHelper.RequestHtml(Client, url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            result = GetDetailUrlFromAvSoxSearchResult(htmlDoc, CID);

            return result;
        }

        /// <summary>
        /// 从AvSox众多搜索结果中搜索出符合条件的
        /// </summary>
        /// <returns></returns>
        private string GetDetailUrlFromAvSoxSearchResult(HtmlDocument htmlDoc, string CID)
        {
            string result = null;

            //是否提示搜索失败
            var alertNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'alert-danger')]");
            if (alertNode != null) return null;

            var SearchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='waterfall']/div[@class='item']");
            //搜索无果，退出
            if (SearchResultNodes == null) return null;

            //分割通过正则匹配得到的CID
            var spliteResult = SpliteLocalCID(CID);
            if (spliteResult == null) return null;

            string left_cid = spliteResult.Item1;
            string right_cid = spliteResult.Item2;

            string search_left_cid;
            string search_right_cid;
            for (var i = 0; i < SearchResultNodes.Count; i++)
            {
                var movie_list = SearchResultNodes[i];
                var title_search = movie_list.SelectSingleNode(".//div[@class='photo-info']/span/date")?.InnerText;
                if (title_search == null) continue;

                string title = title_search.ToUpper();

                var split_result = title.Split(new char[] { '-', '_' });
                //没有分隔符，尝试正则匹配（n167）
                if (split_result.Length == 1)
                {
                    var match_result = Regex.Match(title, @"^([A-Z]+)(\d+)$");
                    if (match_result == null) continue;
                    search_left_cid = match_result.Groups[1].Value;
                    search_right_cid = match_result.Groups[2].Value;
                }
                //有且只有两个分隔符（HEYZO-2934）
                else if (split_result.Length == 2)
                {
                    search_left_cid = split_result[0];
                    search_right_cid = split_result[1];
                }
                //有且有三个分隔符（FC2-PPV-3143749）
                else if (split_result.Length == 3)
                {
                    if (CID.Contains("FC2"))
                    {
                        search_left_cid = "FC2";
                        search_right_cid = split_result[2];
                    }
                    else
                        return null;
                }
                //超过三个，预料之外的情况
                else
                    return null;

                int currentNum;
                int searchNum;

                if (search_left_cid == left_cid
                         && (search_right_cid == right_cid
                                || (Int32.TryParse(right_cid, out currentNum)
                                        && Int32.TryParse(right_cid, out searchNum)
                                            && currentNum.Equals(searchNum))))
                {
                    var detail_url = SearchResultNodes[i].SelectSingleNode(".//a[contains(@class,'movie-box')]").Attributes["href"].Value;

                    //只有“//”没有“http(s)://”，补充上
                    if (!detail_url.Contains("http") && detail_url.Contains("//"))
                    {
                        detail_url = $"https:{detail_url}";
                    }

                    result = detail_url;
                    break;
                }
                else
                    continue;
            }

            return result;
        }

        /// <summary>
        /// 从javdb中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromJavDB(string CID)
        {

            string JavDBUrl = AppSettings.JavDB_BaseUrl;
            string SavePath = AppSettings.Image_SavePath;

            CID = CID.ToUpper();

            var detail_url = await GetDetailUrlFromJavDBSearchResult(CID);
            //搜索无果，退出
            if (detail_url == null) return null;

            string strResult;

            //访问fc内容需要cookie
            if (CID.Contains("FC"))
            {
                //javdb没有登录无法访问fc内容
                if (string.IsNullOrEmpty(AppSettings.javdb_Cookie))
                {
                    return null;
                }

                if (ClientWithJavDBCookie == null)
                {
                    ClientWithJavDBCookie = CreateClient(new Dictionary<string, string>() {
                        {"cookie",AppSettings.javdb_Cookie },
                        {"user-agent" ,BrowserUserAgent}
                    });
                }

                strResult = await RequestHelper.RequestHtml(ClientWithJavDBCookie, detail_url);
            }
            else
            {
                strResult = await RequestHelper.RequestHtml(Client, detail_url);
            }

            if (string.IsNullOrEmpty(strResult)) return null;

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
                ImageUrl = UrlCombine(JavDBUrl, ImageUrl);
            }

            var AttributeNodes = video_meta_panelNode.SelectNodes(".//div[contains(@class,'panel-block')]");

            videoInfo.truename = CID;
            //信息
            for (var i = 0; i < AttributeNodes.Count; i++)
            {
                var keyNode = AttributeNodes[i].SelectSingleNode("strong");
                if (keyNode == null) continue;
                string key = keyNode.InnerText;

                var valueNode = AttributeNodes[i].SelectSingleNode("span");

                ////以网页的CID为准
                //if (key.Contains("番號"))
                //{
                //    videoInfo.truename = valueNode.InnerText;
                //    CID = videoInfo.truename;
                //}
                if (key.Contains("日期"))
                {
                    videoInfo.releasetime = valueNode.InnerText;
                }
                else if (key.Contains("時長"))
                {
                    videoInfo.lengthtime = valueNode.InnerText.Trim().Replace(" 分鍾", "分钟");
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

            //标题
            var TitleNode = htmlDoc.DocumentNode.SelectSingleNode(".//strong[@class='current-title']");
            var title = TitleNode.InnerText;
            videoInfo.title = title.Replace(videoInfo.truename, "").Trim();

            ////下载封面
            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await downloadFile(ImageUrl, filePath, CID);

            //样品图片
            var preview_imagesSingesNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'preview-images')]");
            if (preview_imagesSingesNode != null)
            {
                var preview_imagesNodes = preview_imagesSingesNode.SelectNodes(".//a[@class='tile-item']");
                List<string> sampleUrlList = new();
                if (preview_imagesNodes != null)
                {
                    foreach (var node in preview_imagesNodes)
                    {
                        var sampleImageUrl = node.Attributes["href"].Value;
                        if (!sampleImageUrl.Contains("http"))
                        {
                            sampleImageUrl = UrlCombine(JavDBUrl, sampleImageUrl);
                        }
                        sampleUrlList.Add(sampleImageUrl);
                    }
                    videoInfo.sampleImageList = string.Join(",", sampleUrlList);
                }
            }
            return videoInfo;
        }

        /// <summary>
        /// 向javdb发送搜索请求
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private async Task<string> GetDetailUrlFromJavDBSearchResult(string CID)
        {
            string JavDBUrl = AppSettings.JavDB_BaseUrl;
            string result;

            string url = $"{JavDBUrl}/search?q={CID}&f=all";

            // 访问
            string strResult = await RequestHelper.RequestHtml(Client, url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(strResult);

            //添加此信息会卡住UI
            //FailItems.Add(strResult.Length.ToString());
            //FailItems.Add(strResult.Replace("\n", ""));

            result = GetDetailUrlFromJavDBSearchResult(htmlDoc, CID);

            return result;
        }

        /// <summary>
        /// 从JavDB众多搜索结果中搜索出符合条件的
        /// </summary>
        /// <returns></returns>
        private string GetDetailUrlFromJavDBSearchResult(HtmlDocument htmlDoc, string CID)
        {
            string result = null;

            var SearchResultNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'movie-list')]");

            //搜索无果，退出
            if (SearchResultNodes == null) return null;

            //分割通过正则匹配得到的CID
            var spliteResult = SpliteLocalCID(CID);
            if(spliteResult == null) return null;

            string left_cid = spliteResult.Item1;
            string right_cid = spliteResult.Item2;

            string search_left_cid;
            string search_right_cid;
            for (var i = 0; i < SearchResultNodes.Count; i++)
            {
                var movie_list = SearchResultNodes[i];
                var title_search = movie_list.SelectSingleNode(".//div[@class='video-title']/strong").InnerText;
                string title = title_search.ToUpper();

                var split_result = title.Split(new char[] { '-', '_' });
                if (split_result.Length == 1)
                {
                    var match_result = Regex.Match(title, @"([A-Z]+)(\d+)");
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
                    else
                        return null;
                }
                else
                    return null;

                int currentNum;
                int searchNum;

                if (search_left_cid == left_cid
                         && (search_right_cid == right_cid
                                || (Int32.TryParse(right_cid, out currentNum)
                                        && Int32.TryParse(right_cid, out searchNum)
                                            && currentNum.Equals(searchNum))))
                {
                    var detail_url = SearchResultNodes[i].SelectSingleNode(".//a").Attributes["href"].Value;
                    detail_url = UrlCombine(AppSettings.JavDB_BaseUrl, detail_url);
                    result = detail_url;
                    break;
                }
                else
                {

                }
            }

            return result;
        }

        private Tuple<string,string> SpliteLocalCID(string CID)
        {
            string left_cid;
            string right_cid;

            var split_result = CID.Split(new char[] { '-', '_' });
            if (split_result.Length == 1)
            {
                var match_result = Regex.Match(CID, @"([A-Z]+)(\d+)");
                if (match_result == null) return null;

                left_cid = match_result.Groups[1].Value;
                right_cid = match_result.Groups[2].Value;
            }
            else if (split_result.Length == 2)
            {
                left_cid = split_result[0];
                right_cid = split_result[1];
            }
            else if (split_result.Length == 3)
            {
                if (CID.Contains("HEYDOUGA"))
                {
                    left_cid = split_result[1];
                    right_cid = split_result[2];
                }
                else
                    return null;
            }
            else
                return null;

            return new Tuple<string,string>(left_cid,right_cid);
        }

        /// <summary>
        /// 下载文件，并返回文件路径
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="fileName">文件的名称</param>
        /// <param name="isReplaceExistsImage">是否取代存在的文件，默认为否</param>
        /// <param name="headers">需要的header，可选</param>
        /// <returns>下载后的文件路径</returns>
        public static async Task<string> downloadFile(string url, string filePath, string fileName, bool isReplaceExistsImage = false, Dictionary<string, string> headers = null)
        {

            string localFilename;
            if (fileName.Contains("."))
            {
                localFilename = fileName;
            }
            else
            {
                localFilename = $"{fileName}{Path.GetExtension(url)}";

                if (localFilename.Contains("?"))
                {
                    localFilename = localFilename.Split("?")[0];
                }
            }

            string localPath = Path.Combine(filePath, localFilename);

            bool isSuccessDown = false;

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            //不存在
            if (!File.Exists(localPath) || isReplaceExistsImage)
            {
                //HttpClient Client = CreateClient(headers);

                int maxTryCount = 3;

                for (int i = 0; i < maxTryCount; i++)
                {
                    try
                    {
                        byte[] fileBytes = await Client.GetByteArrayAsync(url);

                        File.WriteAllBytes(localPath, fileBytes);
                        isSuccessDown = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"下载文件时发生错误：{ex.Message}");
                    }
                }
            }
            //存在
            else
            {
                isSuccessDown = true;
            }

            if (isSuccessDown)
                return localPath;
            else
                return null;

        }

    }
}
