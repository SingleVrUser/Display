using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Dto.OneOneFive;
using Display.Models.Entities.OneOneFive;
using Display.Models.Vo;
using Display.Providers.Downloader;

namespace Display.Providers.Spider;

public class Common
{
    public static readonly HttpClient Client = NetworkHelper.CommonClient;

    public static Tuple<string, string> SplitCid(string cid, bool needSingleKeyword = false)
    {
        string leftCid = null;
        string rightCid = null;

        if (cid.Contains("H_"))
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

                if (matchResult.Success)
                {
                    leftCid = matchResult.Groups[1].Value;
                    rightCid = matchResult.Groups[2].Value;
                }
                // 纯字母或纯数字
                else if (needSingleKeyword)
                {
                    return new Tuple<string, string>(cid, null);
                }
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

    public static bool IsSearchResultMatch(string leftCid, string rightCid, string upperText)
    {
        // 输入的只有一个, 只要包含即匹配
        if (rightCid == null)
        {
            if (!upperText.Contains(leftCid)) return false;
        }
        //精确匹配
        else
        {
            var matchResult = Regex.Match(upperText, @$"({leftCid}).*?0?(\d+)");
            if (!matchResult.Success) return false;

            var searchLeftCid = matchResult.Groups[1].Value;
            var searchRightCid = matchResult.Groups[2].Value;

            if (searchLeftCid != leftCid
                || searchRightCid != rightCid
                && (!int.TryParse(rightCid, out var currentNum)
                    || !int.TryParse(searchRightCid, out var searchNum)
                    || !currentNum.Equals(searchNum)))
                return false;
        }

        return true;
    }

    public static async Task<VideoInfo> AnalysisHtmlDocInfoFromAvSoxOrAvMoo(string cid, string detailUrl, HtmlDocument htmlDoc)
    {
        VideoInfo videoInfo = new()
        {
            TrueName = cid,
            Url = detailUrl
        };

        //封面图
        var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='bigImage']");
        if (imageNode == null) return null;
        var coverUrl = imageNode.Attributes["href"].Value;
        videoInfo.ImageUrl = coverUrl;

        //标题（AvMoo在a标签上，AvSox在img标签上）
        var result = imageNode.GetAttributeValue("title", string.Empty);
        if (!string.IsNullOrEmpty(result))
        {
            videoInfo.Title = result;
        }
        else
        {
            var imgNode = imageNode.SelectSingleNode(".//img");

            if (imgNode == null) return null;

            result = imgNode.GetAttributeValue("title", string.Empty);

            if (string.IsNullOrEmpty(result))
                return null;

            videoInfo.Title = result;
        }

        //其他信息
        var infoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='row movie']/div[@class='col-md-3 info']");

        //解析
        var infos = new Dictionary<string, string>();
        var key = string.Empty;
        var values = new List<string>();
        foreach (var info in infoNode.ChildNodes)
        {
            var name = info.Name;

            if (name != "p") continue;

            if (info.HasAttributes)
            {
                var className = info.GetAttributeValue("class", string.Empty);

                var innerText = info.InnerText.Replace(":", string.Empty).Trim();

                if (className == "header")
                {
                    //前一key已检索完成
                    if (values.Count != 0)
                    {
                        infos[key] = string.Join(",", values);

                        values.Clear();
                    }

                    key = innerText;
                }
                else if (!string.IsNullOrEmpty(innerText))
                {
                    values.Add(innerText);
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
                    videoInfo.LengthTime = info.Value;
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
        var actorNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='avatar-waterfall']/a[@class='avatar-box']/span");
        if (actorNodes != null)
            videoInfo.Actor = string.Join(",", actorNodes.Select(item => item.InnerText.Trim()).ToList());

        //样品图片
        var sampleUrlList = new List<string>();
        var sampleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='sample-waterfall']/a[@class='sample-box']");

        if (sampleNodes != null)
        {
            sampleUrlList.AddRange(sampleNodes.Select(sampleNode => sampleNode.GetAttributeValue("href", string.Empty)));

            videoInfo.SampleImageList = string.Join(",", sampleUrlList);
        }

        //下载图片
        var filePath = Path.Combine(AppSettings.ImageSavePath, cid);
        videoInfo.ImageUrl = coverUrl;
        videoInfo.ImagePath = await DbNetworkHelper.DownloadFile(coverUrl, filePath, cid);

        return videoInfo;
    }

}
