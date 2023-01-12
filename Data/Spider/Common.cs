using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Data.Spider;

public class Common
{
    public readonly static HttpClient Client = GetInfoFromNetwork.Client;

    public static Tuple<string, string> SpliteCID(string CID)
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

        return new Tuple<string, string>(left_cid, right_cid);
    }

    public static async Task<VideoInfo> AnalysisHtmlDocInfoFromAvSoxOrAvMoo(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        VideoInfo videoInfo = new();
        videoInfo.truename = CID;
        videoInfo.busurl = detail_url;

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

            if (ImgNode == null) return null;

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
        videoInfo.imagepath = await GetInfoFromNetwork.downloadFile(CoverUrl, filePath, CID);

        return videoInfo;
    }

}
