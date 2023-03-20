
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Display.Helper;

namespace Display.Data;

public class GetActorInfoFromNetwork
{
    public static async Task<ActorInfo> SearchInfoFromMinnanoAv(string name)
    {
        string baseUrl = AppSettings.MinnanoAv_BaseUrl;

        string url = GetInfoFromNetwork.UrlCombine(baseUrl, $"search_result.php?search_scope=actress&search_word={name}&search=+Go+");

        var Client = GetInfoFromNetwork.Client;

        ActorInfo actorInfo = new() { name = name };

        Tuple<string, string> result = await RequestHelper.RequestHtml(Client, url);
        if (result == null) return null;

        actorInfo.info_url = result.Item1;

        string htmlString = result.Item2;

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);

        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section/section/section/h1");
        if (titleNode == null || titleNode.InnerText.Contains("検索結果")) return null;

        var profileNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/section/section/section/div[@class='act-profile']/table/tr");
        if (profileNodes == null) return null;


        List<string> otherNames = new List<string>();

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
                        actorInfo.birthday = match.Groups[0].Value;
                    }
                    break;
                case "サイズ":
                    match = Regex.Match(value, @"T(\d*) / B(\d*).* / W(\d*) / H(\d*)");
                    if (match.Success)
                    {
                        actorInfo.height = string.IsNullOrEmpty(match.Groups[1].Value) ? 0 : Convert.ToInt32(match.Groups[1].Value);
                        actorInfo.bust = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
                        actorInfo.waist = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : Convert.ToInt32(match.Groups[3].Value);
                        actorInfo.hips = string.IsNullOrEmpty(match.Groups[4].Value) ? 0 : Convert.ToInt32(match.Groups[4].Value);

                        actorInfo.bwh = $"{actorInfo.bust}_{actorInfo.waist}_{actorInfo.hips}";
                    }
                    break;
                case "AV出演期間":
                    Console.WriteLine($"出演时间：{value}");
                    actorInfo.work_time = value;
                    break;
                case "ブログ":
                    Console.WriteLine($"博客：{value}");
                    actorInfo.blog_url = value;
                    break;
            }
        }

        Console.WriteLine($"別名：{string.Join(",", otherNames)}");
        actorInfo.otherNames = otherNames;

        var thumNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section/section/section/div[@class='act-area']/div[@class='thumb']/img");
        if (thumNode != null)
        {
            Console.WriteLine($"头像地址：http://www.minnano-av.com{thumNode.GetAttributeValue("src", string.Empty)}");

            string img_url = thumNode.GetAttributeValue("src", string.Empty);

            if (!string.IsNullOrEmpty(img_url))
            {
                actorInfo.image_url = $"{AppSettings.MinnanoAv_BaseUrl}{img_url}";
            }
        }

        return actorInfo;
    }
}
