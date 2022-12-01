using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace Data.Helper;

public class RequestHelper
{
    public static async Task<string> RequestHtml(HttpClient client,string url, int maxRequestCount = 5)
    {
        Uri uri = new Uri(url);

        // 访问
        HttpResponseMessage response;
        string strResult = string.Empty;

        for(int i = 0; i < maxRequestCount; i++)
        {
            try
            {
                response = await client.GetAsync(uri);
                strResult = await response.Content.ReadAsStringAsync();

                break;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"访问网页时发生错误:{ex.Message}");

                //等待一秒后继续
                await Task.Delay(1000);
            }
        }

        return strResult;
    }
}
