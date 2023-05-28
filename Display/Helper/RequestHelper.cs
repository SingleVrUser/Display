using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ABI.Microsoft.UI.Xaml;
using Display.Data;
using Display.Models;
using Newtonsoft.Json;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Display.Helper;

public class RequestHelper
{
    public static async Task<Tuple<string, string>> RequestHtml(HttpClient client, string url, int maxRequestCount = 5)
    {
        // 访问
        HttpResponseMessage response;
        string strResult = string.Empty;

        Tuple<string, string> tuple = null;
        string RequestUrl = string.Empty;

        for (int i = 0; i < maxRequestCount; i++)
        {
            try
            {
                //设置超时时间（5s）
                var option = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

                response = await client.GetAsync(new Uri(url), option);

                RequestUrl = response.RequestMessage.RequestUri.ToString();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    strResult = await response.Content.ReadAsStringAsync();
                    strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

                    break;
                }
                //JavDb访问Fc2需要登录，如果cookie失效，就无法访问
                else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                {
                    if (url.Contains(AppSettings.JavDB_BaseUrl))
                        GetInfoFromNetwork.IsJavDbCookieVisible = false;

                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问网页时发生错误:{ex.Message}");

                //等待一秒后继续
                await Task.Delay(1000);
            }
        }

        if (!string.IsNullOrEmpty(RequestUrl) && !string.IsNullOrEmpty(strResult))
            tuple = new(RequestUrl, strResult);

        return tuple;
    }

    public static async Task<Tuple<string, string>> PostHtml(HttpClient client, string url, Dictionary<string, string> values, int maxRequestCount = 5)
    {
        // 访问
        HttpResponseMessage response;
        string strResult = string.Empty;

        Tuple<string, string> tuple = null;

        var requestUrl = string.Empty;

        var content = new FormUrlEncodedContent(values);

        for (int i = 0; i < maxRequestCount; i++)
        {
            try
            {
                response = client.PostAsync(url, content).Result;

                requestUrl = response.RequestMessage.RequestUri.ToString();

                if (!response.IsSuccessStatusCode) continue;

                strResult = await response.Content.ReadAsStringAsync();

                strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问网页时发生错误:{ex.Message}");

                //等待一秒后继续
                await Task.Delay(1000);
            }
        }

        if (!string.IsNullOrEmpty(requestUrl) && !string.IsNullOrEmpty(strResult))
            tuple = new(requestUrl, strResult);

        return tuple;
    }

    public static string DecodeCfEmail(string data)
    {
        string email = string.Empty;

        var r = Convert.ToInt32(data[..2], 16);

        for (int i = 2; i < data.Length; i += 2)
        {
            email += (char)(Convert.ToInt32(data.Substring(i, 2), 16) ^ r);
        }

        return email;
    }
}
