using Display.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Display.Providers.Downloader;

namespace Display.Helper.Network;

internal static class RequestHelper
{
    public static async Task<Tuple<string, string>> RequestHtml(HttpClient client, string url, CancellationToken token, int maxRequestCount = 3)
    {
        // 访问
        var strResult = string.Empty;

        Tuple<string, string> tuple = null;
        var requestUrl = string.Empty;

        var uri = new Uri(url);

        Debug.WriteLine("正在访问：" + uri.AbsoluteUri);

        for (var i = 0; i < maxRequestCount; i++)
        {
            try
            {
                // 设置超时时间（5s）
                var timeoutCancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

                // 添加额外的退出条件
                var compositeCancel = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancelToken, token);

                var response = await client.GetAsync(uri, compositeCancel.Token);

                requestUrl = response.RequestMessage?.RequestUri?.ToString();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    strResult = await response.Content.ReadAsStringAsync(timeoutCancelToken);
                    strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

                    break;
                }

                //JavDb访问Fc2需要登录，如果cookie失效，就无法访问
                if (response.StatusCode != System.Net.HttpStatusCode.BadGateway) continue;

                // if (url.Contains(AppSettings.JavDbBaseUrl))
                //     GetInfoFromNetwork.IsJavDbCookieVisible = false;

                break;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("任务退出");
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问网页时发生错误:{ex.Message}");

                //等待两秒后继续
                await Task.Delay(2000, token);
            }
        }

        if (!string.IsNullOrEmpty(requestUrl) && !string.IsNullOrEmpty(strResult))
            tuple = new Tuple<string, string>(requestUrl, strResult);

        return tuple;
    }

    public static async Task<Tuple<string, string>> PostHtml(HttpClient client, string url, Dictionary<string, string> values, CancellationToken token, int maxRequestCount = 5)
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
                var response = client.PostAsync(url, content, token).Result;

                requestUrl = response.RequestMessage?.RequestUri?.ToString();

                if (!response.IsSuccessStatusCode) continue;

                strResult = await response.Content.ReadAsStringAsync(token);

                strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

                break;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) return null;

                Debug.WriteLine($"访问网页时发生错误:{ex.Message}");

                //等待两秒后继续
                await Task.Delay(2000, token);
            }
        }

        if (!string.IsNullOrEmpty(requestUrl) && !string.IsNullOrEmpty(strResult))
            tuple = new Tuple<string, string>(requestUrl, strResult);

        return tuple;
    }

    public static string DecodeCfEmail(string data)
    {
        var email = string.Empty;

        var r = Convert.ToInt32(data[..2], 16);

        for (var i = 2; i < data.Length; i += 2)
        {
            email += (char)(Convert.ToInt32(data.Substring(i, 2), 16) ^ r);
        }

        return email;
    }

}
