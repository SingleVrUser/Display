using Display.Data;
using Display.Helper;
using Display.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Lights;
using static System.Int32;

namespace Display.Spider
{
    public class X1080X
    {
        private static string BaseUrl => AppSettings.X1080XBaseUrl;

        public static bool IsOn => AppSettings.IsUseX1080X;

        private static HttpClient _client;

        private static readonly string[] ExpectType = { "4K超清", "高清有碼", "BT綜合區" };

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

        public static async Task<List<Forum1080AttachmentInfo>> GetDownLinkFromUrl(string url)
        {
            var result = await RequestHelper.RequestHtml(Client, url);

            var htmlString = result.Item2;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);

            var links = GetAttmnInfoFromHtml(htmlDoc);

            return links;
        }

        private static List<Forum1080AttachmentInfo> GetAttmnInfoFromHtml(HtmlDocument htmlDoc)
        {
            HashSet<Forum1080AttachmentInfo> links = new();

            // 先查找磁力链接
            var codeNodes = htmlDoc.DocumentNode.SelectNodes("div[contains(@id,'code')]");
            if (codeNodes != null)
            {
                foreach (var node in codeNodes)
                {
                    var codeContent = node.InnerText;
                    foreach (var line in codeContent.Split('\n'))
                    {
                        links.Add(new Forum1080AttachmentInfo(line, AttmnType.Magnet));
                    }
                }
            }

            if (links.Count > 0)
            {
                return links.ToList();
            }

            //附件
            var attmnNodes = htmlDoc.DocumentNode.SelectNodes(".//dl[@class='tattl']");

            // 出售区附件
            if (attmnNodes == null)
            {
                var attmnSpanNode = htmlDoc.DocumentNode.SelectNodes(".//span[contains(@id,'attach_')]");
                if (attmnSpanNode == null)
                {
                    return links.ToList();
                }

                foreach (var spanNode in attmnSpanNode)
                {
                    var nameNode = spanNode.SelectSingleNode("a");

                    var description = spanNode.SelectSingleNode("em")?.InnerText;

                    if (nameNode != null && description != null)
                    {
                        var name = nameNode.InnerText.Trim();

                        if (name.Contains("protected"))
                        {
                            var emailNode = nameNode.SelectSingleNode("span");
                            if (emailNode != null)
                            {
                                var data = emailNode.GetAttributeValue("data-cfemail", string.Empty);

                                if (!string.IsNullOrEmpty(data))
                                {
                                    name = RequestHelper.DecodeCfEmail(data);
                                }
                            }
                        }

                        var downLink = nameNode.GetAttributeValue("href", string.Empty);
                        var attmnType = GetAttmnTypeFromName(name);
                        Forum1080AttachmentInfo attmnInfo = new(downLink, attmnType)
                        {
                            Name = name
                        };

                        var result = Regex.Match(description, "([. \\w]+)  , 下载次数: (\\d+)(.*, 售价: 点数 (\\d+))?");
                        if (result.Success)
                        {
                            attmnInfo.Size = result.Groups[1].Value;

                            if (TryParse(result.Groups[2].Value, out int count))
                            {
                                attmnInfo.DownCount = count;
                            }

                            if (TryParse(result.Groups[4].Value, out int countResult))
                            {
                                attmnInfo.Expense = countResult;
                            }
                        }


                        links.Add(attmnInfo);
                    }

                }

                return links.ToList();
            }
            
            // 普通附件
            foreach (var node in attmnNodes)
            {
                var attmnDd = node.SelectSingleNode("dd");

                var attmnA = attmnDd.SelectSingleNode("p[@class='attnm']/a");
                
                var downLink = attmnA.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(downLink)) continue;

                downLink = downLink.Replace("&amp;", "&");
                var name = attmnA.InnerText.Trim();

                var attmnType = GetAttmnTypeFromName(name);

                Forum1080AttachmentInfo attmnInfo = new(downLink, attmnType)
                {
                    Name = name
                };

                var attmnPStr = attmnDd.SelectNodes("p").FirstOrDefault(x => x.InnerText.Contains("下载次数"));
                if (attmnPStr != null)
                {
                    var result = Regex.Match(attmnPStr.InnerText, "([. \\w]+)  , 下载次数: (\\d+)(.*, 售價: 點數 (\\d+))?");
                    if (result.Success)
                    {
                        attmnInfo.Size = result.Groups[1].Value;

                        if (TryParse(result.Groups[2].Value, out int count))
                        {
                            attmnInfo.DownCount = count;
                        }

                        attmnInfo.Expense = 1;
                    }
                }

                links.Add(attmnInfo);

            }

            return links.ToList();
        }

        private static AttmnType GetAttmnTypeFromName(string name)
        {
            AttmnType attmnType;
            if (name.Contains(".rar"))
            {
                attmnType = AttmnType.Rar;
            }
            else if (name.Contains(".txt"))
            {
                attmnType = AttmnType.Txt;
            }
            else if (name.Contains(".torrent"))
            {
                attmnType = AttmnType.Torrent;
            }
            else
            {
                attmnType = AttmnType.Unknown;
            }

            return attmnType;
        }


        public static async Task<List<Forum1080SearchResult>> GetMatchInfosFromCid(string cid)
        {
            var searchUrl = $"{BaseUrl}search.php?searchsubmit=yes";

            var txt = cid.Replace('-', '+').Replace(' ', '+');

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
            
            return detailInfos;
        }

        private static List<Forum1080SearchResult> GetDetailInfoFromSearchResult(string cid,HtmlDocument htmlDoc)
        {
            var searchResultNodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='pbw']");

            //搜索无果，退出
            if (searchResultNodes == null)
            {
                var tipNode = htmlDoc.DocumentNode.SelectSingleNode("//title");

                if (!tipNode.InnerText.Contains("提示信息")) return null;

                var messageNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='messagetext']");

                var content = messageNode != null ? messageNode.InnerText.Trim() : tipNode.InnerText;

                Toast.tryToast("x1080x出错", content);

                return null;
            }

            //分割通过正则匹配得到的CID
            var splitResult = Common.SpliteCID(cid.ToUpper());
            if (splitResult == null) return null;

            var leftCid = splitResult.Item1 ;
            var rightCid = splitResult.Item2;

            List<Forum1080SearchResult> detailUrlInfos = new();

            for (var i = 0; i < searchResultNodes.Count; i++)
            {
                string searchLeftCid;
                string searchRightCid;

                var pbwNode = searchResultNodes[i];
                var titleNode = pbwNode.SelectSingleNode(".//h3[@class='xs3']/a");
                var title = titleNode.InnerText;
                var upperText = title.ToUpper();

                var matchResult = Regex.Match(upperText, @$"({leftCid})-?0?(\d+)");
                if (matchResult.Success)
                {
                    searchLeftCid = matchResult.Groups[1].Value;
                    searchRightCid = matchResult.Groups[2].Value;
                }
                else
                    continue;

                if (searchLeftCid != leftCid
                    || (searchRightCid != rightCid
                        && (!TryParse(rightCid, out var currentNum)
                            || !TryParse(searchRightCid, out var searchNum)
                            || !currentNum.Equals(searchNum)))) continue;

                var detailUrl = titleNode.GetAttributeValue("href",null);
                if(detailUrl == null) continue;

                detailUrl = detailUrl.Replace("&amp;", "&");

                detailUrlInfos.Add(GetForum1080FromNode(pbwNode, title,detailUrl));
            }

            return detailUrlInfos;
        }

        private static Forum1080SearchResult GetForum1080FromNode(HtmlNode node,string title,string detailUrl)
        {
            Forum1080SearchResult forum1080SearchResult = new()
            {
                Title = title,
                Url = detailUrl
            };
            
            var ps = node.SelectNodes("p");

            if (ps.Count != 3) return forum1080SearchResult;

            forum1080SearchResult.Description = ps[1].InnerText;

            var span = ps[2].SelectNodes("span");

            if(span.Count != 3)return forum1080SearchResult;

            forum1080SearchResult.Time = span[0].InnerText;
            forum1080SearchResult.Author = span[1].InnerText;
            forum1080SearchResult.Type = span[2].InnerText;

            return forum1080SearchResult;
        }
    }

}
