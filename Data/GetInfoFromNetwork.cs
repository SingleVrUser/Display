using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public class GetInfoFromNetwork
    {
        private HttpClient Client;

        public GetInfoFromNetwork()
        {
            Client = new HttpClient();
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");
        }

        public static string UrlCombine(string uri1, string uri2)
        {
            Uri baseUri = new Uri(uri1+"/");
            Uri myUri = new Uri(baseUri, uri2);
            return myUri.ToString();
        }

        public async Task<bool> CheckUrlUseful(string checkUrl)
        {
            bool isUseful = true;
            HttpResponseMessage resp;
            try
            {
                resp = await Client.GetAsync(checkUrl);
            }
            catch(HttpRequestException)
            {
                isUseful = false;
            }

            return isUseful;
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

            HttpClient Client = new HttpClient();
            string busurl = UrlCombine(JavBusUrl,CID);


            Uri uri = new Uri(busurl);
            videoInfo.busurl = busurl;

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
                ImageUrl = UrlCombine(JavBusUrl, ImageUrl);
            }

            var title = ImageUrlNode.Attributes["title"].Value;
            videoInfo.title = title;

            var AttributeNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-md-3 info']//p");

            ////下载封面
            string filePath = Path.Combine(SavePath, CID);
            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await downloadImage(ImageUrl, filePath, CID);
            
            //信息
            for (var i = 0; i < AttributeNodes.Count; i++)
            {
                var AttributeNode = AttributeNodes[i];

                var header = AttributeNode.FirstChild.InnerText.Trim();

                if (header == "識別碼:")
                {
                    videoInfo.truename = AttributeNode.SelectNodes(".//span")[1].InnerText.Trim();
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
        /// 从javdb中搜索影片信息
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        public async Task<VideoInfo> SearchInfoFromJavDB(string CID)
        {
            VideoInfo videoInfo = new VideoInfo();

            string JavDBUrl = AppSettings.JavDB_BaseUrl;
            string SavePath = AppSettings.Image_SavePath;

            CID = CID.ToUpper();

            var detail_url = await GetDetailUrlFromJavDBSearchResult(CID);
            //搜索无果，退出
            if (detail_url == null) return null;

            //两次访问间隔不能太短
            await RandomTimeDelay(3, 6);

            HttpClient Client;
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");

            //访问fc内容需要cookie
            if (CID.Contains("FC"))
            {
                //未设置Cookie，直接退出
                if(AppSettings.javdb_Cookie == null)
                {
                    return null;
                }
                else
                {
                    Client.DefaultRequestHeaders.Add("cookie", AppSettings.javdb_Cookie);
                }

            }

            Uri uri = new Uri(detail_url);

            // 访问
            HttpResponseMessage response ;
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

            videoInfo.busurl = detail_url;

            var ImageUrl = video_meta_panelNode.SelectSingleNode(".//img[@class='video-cover']").Attributes["src"].Value;
            if (!ImageUrl.Contains("http"))
            {
                ImageUrl = UrlCombine(JavDBUrl,ImageUrl);
            }

            ////下载封面
            string filePath = Path.Combine(SavePath, CID);

            videoInfo.imageurl = ImageUrl;
            videoInfo.imagepath = await downloadImage(ImageUrl, filePath, CID);

            var AttributeNodes = video_meta_panelNode.SelectNodes(".//div[contains(@class,'panel-block')]");
            //信息
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

            //标题
            var TitleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='video-detail']//h2//strong");
            var title = TitleNode.InnerText;
            videoInfo.title = title.Replace(videoInfo.truename, "").Trim();

            //样品图片
            var preview_imagesSingesNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'preview-images')]");
            if(preview_imagesSingesNode != null)
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


        private async Task<string> GetDetailUrlFromJavDBSearchResult(string CID)
        {
            string JavDBUrl = AppSettings.JavDB_BaseUrl;
            string result;

            //反爬严重，尝试添加user-agent

            string busurl = $"{JavDBUrl}/search?q={CID}&f=all";
            Uri uri = new Uri(busurl);

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

            //添加此信息会卡住UI
            //FailItems.Add(strResult.Length.ToString());
            //FailItems.Add(strResult.Replace("\n", ""));

            result = GetDetailUrlFromSearchResult(htmlDoc, CID);

            return result;
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

            string left_cid;
            string right_cid;

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
                    detail_url = UrlCombine(AppSettings.JavDB_BaseUrl, detail_url);
                    result = detail_url;
                    break;
                }
            }


            return result;
        }

        /// <summary>
        /// 下载图片，并返回图片路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> downloadImage(string url, string imagePath, string imageName, bool isReplaceExistsImage = false)
        {
            HttpClient Client = new HttpClient();

            string localFilename = $"{imageName}{Path.GetExtension(url)}";
            string localPath = Path.Combine(imagePath, localFilename);

            if (!File.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            if (isReplaceExistsImage || !File.Exists(localPath))
            {
                try
                {
                    byte[] imageBytes = await Client.GetByteArrayAsync(url);

                    File.WriteAllBytes(localPath, imageBytes);
                }
                catch
                {
                    return localPath;
                }
            }

            return localPath;

        }
    }
}
