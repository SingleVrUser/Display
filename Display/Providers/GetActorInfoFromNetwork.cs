using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using HtmlAgilityPack;

namespace Display.Providers;

public static class GetActorInfoFromNetwork
{
    public static async Task<ActorInfo> SearchInfoFromMinnanoAv(string name, CancellationToken token)
    {
        var baseUrl = AppSettings.MinnanoAvBaseUrl;

        var url = NetworkHelper.UrlCombine(baseUrl, $"search_result.php?search_scope=actress&search_word={name}&search=+Go+");

        var client = NetworkHelper.CommonClient;

        ActorInfo actorInfo = new(name);

        var result = await RequestHelper.RequestHtml(client, url, token);
        if (result == null) return null;

        actorInfo.InfoUrl = result.Item1;
        var htmlString = result.Item2;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section/section/section/h1");
        if (titleNode == null || titleNode.InnerText.Contains("検索結果")) return null;

        var profileNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/section/section/section/div[@class='act-profile']/table/tr");
        if (profileNodes == null) return null;

        var otherNames = new List<string>();

        foreach (var profileNode in profileNodes)
        {
            var spanNode = profileNode.SelectSingleNode("./td/span");
            if (spanNode == null) continue;

            var pNode = profileNode.SelectSingleNode("./td/p");
            if (pNode == null || string.IsNullOrEmpty(pNode.InnerText)) continue;

            var key = spanNode.InnerText.Trim();
            var value = pNode.InnerText.Trim();

            switch (key)
            {
                case "別名":
                    otherNames.Add(value.Split('（')[0].Trim());
                    break;
                case "生年月日":
                    var match = Regex.Match(value, @"\d+年\d+月\d+日");
                    if (match.Success)
                    {
                        Debug.WriteLine($"生年月日：{match.Groups[0].Value}");
                        actorInfo.Birthday = match.Groups[0].Value;
                    }
                    break;
                case "サイズ":
                    match = Regex.Match(value, @"T(\d*) / B(\d*).* / W(\d*) / H(\d*)");
                    if (match.Success)
                    {
                        actorInfo.Height = string.IsNullOrEmpty(match.Groups[1].Value) ? 0 : Convert.ToInt32(match.Groups[1].Value);
                        var bust = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
                        var waist = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : Convert.ToInt32(match.Groups[3].Value);
                        var hips = string.IsNullOrEmpty(match.Groups[4].Value) ? 0 : Convert.ToInt32(match.Groups[4].Value);

                        actorInfo.Bwh = new BwhInfo
                        {
                            Bust = bust,
                            Waist = waist,
                            Hips = hips
                        };
                    }
                    break;
                case "AV出演期間":
                    Debug.WriteLine($"出演时间：{value}");
                    actorInfo.WorkTime = value;
                    break;
                case "ブログ":
                    Debug.WriteLine($"博客：{value}");
                    actorInfo.BlogUrl = value;
                    break;
            }
        }

        Debug.WriteLine($"別名：{string.Join(",", otherNames)}");
        
        actorInfo.NameList = otherNames.Select(i=>new ActorName(i)).ToList();

        var thumbNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section/section/section/div[@class='act-area']/div[@class='thumb']/img");
        if (thumbNode == null) return actorInfo;

        Debug.WriteLine($"头像地址：http://www.minnano-av.com{thumbNode.GetAttributeValue("src", string.Empty)}");

        var imgUrl = thumbNode.GetAttributeValue("src", string.Empty);

        if (string.IsNullOrEmpty(imgUrl)) return actorInfo;

        actorInfo.ProfilePath = $"{AppSettings.MinnanoAvBaseUrl}{imgUrl}";
        return actorInfo;
    }
}
