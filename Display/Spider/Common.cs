using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Display.Data;

namespace Display.Spider;

public class Common
{
    public static readonly HttpClient Client = GetInfoFromNetwork.CommonClient;

    public static Tuple<string, string> SplitCid(string cid)
    {
        string leftCid;
        string rightCid;

        if(cid.Contains("H_"))
        {
            var matchResult = Regex.Match(cid, @"H_\d+([A-Z]+)(\d+)");

            leftCid = matchResult.Groups[1].Value;
            rightCid = matchResult.Groups[2].Value;
        }
        else
        {
            var splitResult = cid.Split('-', '_');
            if (splitResult.Length == 1)
            {
                var matchResult = Regex.Match(cid, @"([A-Z]+)(\d+)");

                leftCid = matchResult.Groups[1].Value;
                rightCid = matchResult.Groups[2].Value;
            }
            else if (splitResult.Length == 2)
            {
                leftCid = splitResult[0];
                rightCid = splitResult[1];
            }
            else if (splitResult.Length == 3)
            {
                if (cid.Contains("HEYDOUGA"))
                {
                    leftCid = splitResult[1];
                    rightCid = splitResult[2];
                }
                else
                    return null;
            }
            else
                return null;
        }

        return new Tuple<string, string>(leftCid, rightCid);
    }

    public static async Task<VideoInfo> AnalysisHtmlDocInfoFromAvSoxOrAvMoo(string CID, string detail_url, HtmlDocument htmlDoc)
    {
        VideoInfo videoInfo = new();
        videoInfo.trueName = CID;
        videoInfo.busUrl = detail_url;

        //封面图
        string CoverUrl = null;
        var ImageNode = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='bigImage']");
        if (ImageNode == null) return null;
        CoverUrl = ImageNode.Attributes["href"].Value;
        videoInfo.ImageUrl = CoverUrl;

        //标题（AvMoox在a标签上，AvSox在img标签上）
        var result = ImageNode.GetAttributeValue("title", string.Empty);
        if (!string.IsNullOrEmpty(result))
        {
            videoInfo.Title = result;
        }
        else
        {
            var ImgNode = ImageNode.SelectSingleNode(".//img");

            if (ImgNode == null) return null;

            result = ImgNode.GetAttributeValue("title", string.Empty);

            if (string.IsNullOrEmpty(result))
                return null;

            videoInfo.Title = result;
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

                    var childInnerText = child.InnerText.Replace(":", string.Empty).Trim();

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
                    videoInfo.ReleaseTime = info.Value;
                    break;
                case "长度":
                    videoInfo.Lengthtime = info.Value;
                    break;
                case "导演":
                    videoInfo.Director = info.Value;
                    break;
                case "制作商":
                    videoInfo.Producer = info.Value;
                    break;
                case "发行商":
                    videoInfo.Publisher = info.Value;
                    break;
                case "系列":
                    videoInfo.Series = info.Value;
                    break;
                case "类别":
                    videoInfo.Category = info.Value;
                    break;
            }
        }

        //演员
        var ActorNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='avatar-waterfall']/a[@class='avatar-box']/span");
        if (ActorNodes != null)
            videoInfo.Actor = string.Join(",", ActorNodes.Select(item => item.InnerText.Trim()).ToList());

        //样品图片
        List<string> sampleUrlList = new List<string>();
        var sampleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='sample-waterfall']/a[@class='sample-box']");

        if (sampleNodes != null)
        {
            foreach (var sampleNode in sampleNodes)
            {
                sampleUrlList.Add(sampleNode.GetAttributeValue("href", string.Empty));
            }

            videoInfo.SampleImageList = string.Join(",", sampleUrlList);
        }

        //下载图片
        string filePath = Path.Combine(AppSettings.ImageSavePath, CID);
        videoInfo.ImageUrl = CoverUrl;
        videoInfo.ImagePath = await GetInfoFromNetwork.DownloadFile(CoverUrl, filePath, CID);

        return videoInfo;
    }

}
