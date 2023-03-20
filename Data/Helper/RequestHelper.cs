using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Data.Helper;

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
                response = await client.GetAsync(new Uri(url));

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
                        GetInfoFromNetwork.IsJavDbCookieVisiable = false;

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

        string RequestUrl = string.Empty;

        var content = new FormUrlEncodedContent(values);

        for (int i = 0; i < maxRequestCount; i++)
        {
            try
            {
                response = client.PostAsync(url, content).Result;

                RequestUrl = response.RequestMessage.RequestUri.ToString();

                if (response.IsSuccessStatusCode)
                {
                    strResult = await response.Content.ReadAsStringAsync();

                    strResult = strResult.Replace("\r", "").Replace("\n", "").Trim();

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
}
