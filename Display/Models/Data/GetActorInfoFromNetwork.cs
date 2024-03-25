
using Display.Helper.Network;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Models.Data;

public class GetActorInfoFromNetwork
{
    public static async Task<ActorInfo> SearchInfoFromMinnanoAv(string name, CancellationToken token)
    {
        var baseUrl = AppSettings.MinnanoAvBaseUrl;

        var url = GetInfoFromNetwork.UrlCombine(baseUrl, $"search_result.php?search_scope=actress&search_word={name}&search=+Go+");

        var client = GetInfoFromNetwork.CommonClient;

        ActorInfo actorInfo = new() { Name = name };

        Tuple<string, string> result = await RequestHelper.RequestHtml(client, url, token);
        if (result == null) return null;

        actorInfo.InfoUrl = result.Item1;

        string htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
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
                    Match match = Regex.Match(value, @"\d+年\d+月\d+日");
                    if (match.Success)
                    {
                        Console.WriteLine($"生年月日：{match.Groups[0].Value}");
                        actorInfo.Birthday = match.Groups[0].Value;
                    }
                    break;
                case "サイズ":
                    match = Regex.Match(value, @"T(\d*) / B(\d*).* / W(\d*) / H(\d*)");
                    if (match.Success)
                    {
                        actorInfo.Height = string.IsNullOrEmpty(match.Groups[1].Value) ? 0 : Convert.ToInt32(match.Groups[1].Value);
                        actorInfo.Bust = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
                        actorInfo.Waist = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : Convert.ToInt32(match.Groups[3].Value);
                        actorInfo.Hips = string.IsNullOrEmpty(match.Groups[4].Value) ? 0 : Convert.ToInt32(match.Groups[4].Value);

                        actorInfo.Bwh = $"{actorInfo.Bust}_{actorInfo.Waist}_{actorInfo.Hips}";
                    }
                    break;
                case "AV出演期間":
                    Console.WriteLine($"出演时间：{value}");
                    actorInfo.WorkTime = value;
                    break;
                case "ブログ":
                    Console.WriteLine($"博客：{value}");
                    actorInfo.BlogUrl = value;
                    break;
            }
        }

        Console.WriteLine($"別名：{string.Join(",", otherNames)}");
        actorInfo.OtherNames = otherNames;

        var thumNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section/section/section/div[@class='act-area']/div[@class='thumb']/img");
        if (thumNode != null)
        {
            Console.WriteLine($"头像地址：http://www.minnano-av.com{thumNode.GetAttributeValue("src", string.Empty)}");

            string img_url = thumNode.GetAttributeValue("src", string.Empty);

            if (!string.IsNullOrEmpty(img_url))
            {
                actorInfo.ImageUrl = $"{AppSettings.MinnanoAvBaseUrl}{img_url}";
            }
        }

        return actorInfo;
    }
}
