using Display.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Helper;

public class RequestHelper
{
    public static async Task<Tuple<string, string>> RequestHtml(HttpClient client, string url, int maxRequestCount = 5)
    {
        // 访问
        var strResult = string.Empty;

        Tuple<string, string> tuple = null;
        var requestUrl = string.Empty;

        for (int i = 0; i < maxRequestCount; i++)
        {
            try
            {
                //设置超时时间（5s）
                var option = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

                var response = await client.GetAsync(new Uri(url), option);

                requestUrl = response.RequestMessage?.RequestUri?.ToString();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    strResult = await response.Content.ReadAsStringAsync(option);
                    strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

                    break;
                }

                //JavDb访问Fc2需要登录，如果cookie失效，就无法访问
                if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                {
                    if (url.Contains(AppSettings.JavDbBaseUrl))
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

        if (!string.IsNullOrEmpty(requestUrl) && !string.IsNullOrEmpty(strResult))
            tuple = new Tuple<string, string>(requestUrl, strResult);

        return tuple;
    }

    public static async Task<Tuple<string, string>> PostHtml(HttpClient client, string url, Dictionary<string, string> values, int maxRequestCount = 5)
    {
        // 访问
        var strResult = string.Empty;

        Tuple<string, string> tuple = null;

        var requestUrl = string.Empty;

        var content = new FormUrlEncodedContent(values);

        for (var i = 0; i < maxRequestCount; i++)
        {
            try
            {
                var response = client.PostAsync(url, content).Result;

                requestUrl = response.RequestMessage?.RequestUri?.ToString();

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
            tuple = new Tuple<string, string>(requestUrl, strResult);

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
