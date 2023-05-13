using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Common.Parsers.Markdown.Inlines;
using Display.Data;
using Display.Helper;
using Display.Models;
using HtmlAgilityPack;
using static System.Int32;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Display.Spider
{
    public class X1080X
    {
        private static string BaseUrl => AppSettings.X1080XBaseUrl;

        public static bool IsOn => AppSettings.IsUseX1080X;

        private static HttpClient _client;

        public static HttpClient Client
        {
            get
            {
                var headers = new Dictionary<string, string>
                {
                    {"user-agent",AppSettings.X1080XUa},
                    {"cookie",AppSettings.X1080XCookie},
                    {"referer",$"{BaseUrl}forum.php"}
                };
                return _client ??= GetInfoFromNetwork.CreateClient(headers);
            }
            set=> _client = value;
        }

        public static void TryChangedClientHeader(string key, string value)
        {
            if(_client == null) return;

            if (_client.DefaultRequestHeaders.Contains(key))
            {
                _client.DefaultRequestHeaders.Remove(key);
            }

            _client.DefaultRequestHeaders.Add(key,value);

        }

        public static async Task<List<string>> SearchDownLinkFromCid(string cid)
        {
            var info = await GetDetailUrlFromCid(cid);
            if(info == null) return null;

            var detailUrl = $"{BaseUrl}forum.php?mod=viewthread&tid={info.Id}";

            var result = await RequestHelper.RequestHtml(Client, detailUrl);
            if (result == null) return null;

            var htmlString = result.Item2;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            var links = GetDownLinkFromHtml(htmlDoc);

            return links;
        }

        private static List<string> GetDownLinkFromHtml(HtmlDocument htmlDoc)
        {
            HashSet<string> links = new();

            // 先查找磁力链接
            var codeNodes = htmlDoc.DocumentNode.SelectNodes("div[contains(@id,'code')]");
            foreach (var node in codeNodes)
            {
                var codeContent = node.InnerText;
                foreach (var line in codeContent.Split('\n'))
                {
                    links.Add(line);
                }
            }

            if (links.Count > 0)
            {
                return links.ToList();
            }

            //附件
            var attnmNodes = htmlDoc.DocumentNode.SelectNodes(".//dl[@class='tattl']");
            if (attnmNodes == null) return links.ToList();

            foreach (var node in attnmNodes)
            {
                var attnmDd = node.SelectSingleNode("dd");

                var attnmA = attnmDd.SelectSingleNode("p[@class='attnm']/a");
                
                var name = attnmA.InnerText.Trim();
                var downLink = attnmA.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(downLink))
                {
                    links.Add(downLink);
                }
            }

            return links.ToList();
        }


        private static readonly string[] ExpectType = { "4K超清", "高清有碼", "BT綜合區" };

        public static async Task<Forum1080> GetDetailUrlFromCid(string cid)
        {
            var searchUrl = $"{BaseUrl}search.php?searchsubmit=yes";

            var txt = cid.Replace('-', '+');

            var postValues = new Dictionary<string, string>
            {
                //{ "mod", "search"},
                { "formhash", "b930ec86"},
                { "srchtype", "title"},
                { "srchtxt", txt},
                { "mod", "forum"},
                { "searchsubmit", "true"},
            };
            var result = await RequestHelper.PostHtml(Client, searchUrl, postValues);
            if (result == null) return null;

            //var detailUrl = result.Item1;
            var htmlString = result.Item2;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            var detailInfos = GetDetailInfoFromSearchResult(cid, htmlDoc);

            if (detailInfos == null || detailInfos.Count == 0) return null;

            if (detailInfos.Count == 1)
            {
                return detailInfos.FirstOrDefault();
            }

            foreach (var expect in ExpectType.Select(type => detailInfos.FirstOrDefault(x => x.Type == type)).Where(expect => expect != null))
            {
                return expect;
            }

            return detailInfos.FirstOrDefault();
        }

        private static List<Forum1080> GetDetailInfoFromSearchResult(string cid,HtmlDocument htmlDoc)
        {
            var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='pbw']");

            //搜索无果，退出
            if (searchResultNodes == null)
            {
                var tipNode = htmlDoc.DocumentNode.SelectSingleNode("//title");

                if (!tipNode.InnerText.Contains("提示信息")) return null;

                string content;

                var messageNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='messagetext']");

                content = messageNode != null ? messageNode.InnerText.Trim() : tipNode.InnerText;

                Toast.tryToast("x1080x出错", content);

                return null;
            }

            //分割通过正则匹配得到的CID
            var splitResult = Common.SpliteCID(cid);
            if (splitResult == null) return null;

            var leftCid = splitResult.Item1;
            var rightCid = splitResult.Item2;

            List<Forum1080> detailUrlInfos = new();

            for (var i = 0; i < searchResultNodes.Count; i++)
            {
                var searchLeftCid = string.Empty;
                var searchRightCid = string.Empty;

                var pbwNode = searchResultNodes[i];
                var titleNode = pbwNode.SelectSingleNode(".//h3[@class='xs3']/a");
                var title = titleNode.InnerText;
                var upperText = title.ToUpper();

                var split_result = upperText.Split(new char[] { '-', '_' });
                if (split_result.Length == 1)
                {
                    var match_result = Regex.Match(upperText, @"([A-Z]+)(\d+)");
                    if (match_result.Success)
                    {
                        searchLeftCid = match_result.Groups[1].Value;
                        searchRightCid = match_result.Groups[2].Value;
                    }
                }
                else if (split_result.Length == 2)
                {
                    searchLeftCid = split_result[0];
                    searchRightCid = split_result[1];
                }
                else if (split_result.Length == 3)
                {
                    if (upperText.Contains("HEYDOUGA"))
                    {
                        searchLeftCid = split_result[1];
                        searchRightCid = split_result[2];
                    }
                    else
                        return null;
                }
                else
                    return null;

                if (searchLeftCid != leftCid
                    || (searchRightCid != rightCid
                        && (!TryParse(rightCid, out var currentNum)
                            || !TryParse(searchRightCid, out var searchNum)
                            || !currentNum.Equals(searchNum)))) continue;

                var detailUrl = titleNode.GetAttributeValue("href",null);
                if(detailUrl == null) continue;

                detailUrlInfos.Add(GetForum1080FromNode(pbwNode, title));
                break;
            }

            return detailUrlInfos;
        }

        private static Forum1080 GetForum1080FromNode(HtmlNode node,string title)
        {
            Forum1080 forum1080 = new()
            {
                Title = title
            };

            var ps = node.SelectNodes("p");

            if (ps.Count != 3) return forum1080;

            forum1080.Description = ps[0].InnerText;
            forum1080.Time = ps[1].InnerText;
            forum1080.Author = ps[2].InnerText;
            forum1080.Type = ps[3].InnerText;

            return forum1080;
        }
    }

}
