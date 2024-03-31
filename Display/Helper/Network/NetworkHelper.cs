using Display.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Display.Models.Api.OneOneFive.File;

namespace Display.Helper.Network;

internal static class NetworkHelper
{
    private static readonly HttpClient Client = WebApi.GlobalWebApi.Client;

    public static async Task<WebFileInfo> GetFileAsync(long? cid, int limit = 40, int offset = 0, bool useApi2 = false, bool loadAll = false, string orderBy = "user_ptime", int asc = 0, bool isOnlyFolder = false)
    {
        var url = !useApi2 ? $"https://webapi.115.com/files?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json" :
            //旧接口只有t，没有修改时间（te），创建时间（tp）
            $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";

        if (isOnlyFolder)
            url += "&nf=1";

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await Client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        var webFileInfoResult = JsonConvert.DeserializeObject<WebFileInfo>(content);

        if (webFileInfoResult == null) return null;

        //接口1出错，使用接口2
        if (webFileInfoResult.ErrNo == 20130827 && useApi2 == false)
        {
            webFileInfoResult = await GetFileAsync(cid, limit, offset, true, loadAll, webFileInfoResult.Order, webFileInfoResult.IsAsc, isOnlyFolder: isOnlyFolder);
        }
        //需要加载全部，但未加载全部
        else if (loadAll && webFileInfoResult.Count > limit)
        {
            webFileInfoResult = await GetFileAsync(cid, webFileInfoResult.Count, offset, useApi2, true, orderBy, asc);
        }

        return webFileInfoResult;
    }

    public static async Task<ImageInfo> GetImageInfo(string pickCode)
    {
        var url = $"https://webapi.115.com/files/image?pickcode={pickCode}&_={DateTimeOffset.Now.ToUnixTimeSeconds()}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await Client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(result)) return null;

        return JsonConvert.DeserializeObject<ImageInfo>(result);
    }

    public static async Task<Tuple<Stream, long>> GetStreamFromUrl(string url, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        var tmpSize = response.Content.Headers.ContentLength;
        if (tmpSize == null) return null;

        var size = (long)tmpSize;
        return !response.IsSuccessStatusCode ? null : Tuple.Create(await response.Content.ReadAsStreamAsync(token), size);
    }

    public static async Task<IRandomAccessStream> GetIRandomAccessStreamFromUrl(string url, IProgress<int> progress, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        if (!response.IsSuccessStatusCode) return null;

        var tmpSize = response.Content.Headers.ContentLength;
        if (tmpSize == null) return null;

        var size = (long)tmpSize;
        await using var stream = await response.Content.ReadAsStreamAsync(token);

        var buffer = new byte[81920];

        int bytesRead;
        var position = 0;

        var memStream = new MemoryStream();
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
        {
            await memStream.WriteAsync(buffer, 0, bytesRead, token);

            position += bytesRead;
            progress?.Report((int)((double)position / size * 100));
        }

        memStream.Position = 0;
        return memStream.AsRandomAccessStream();
    }

    public static string UrlCombine(string uri1, string uri2)
    {
        var baseUri = new Uri(uri1);
        var myUri = new Uri(baseUri, uri2);
        return myUri.ToString();
    }

    /// <summary>
    /// 检查是否能访问该网页
    /// </summary>
    /// <param name="checkUrl"></param>
    /// <returns></returns>
    public static async Task<bool> CheckUrlUseful(string checkUrl)
    {
        var isUseful = true;
        try
        {
            await CommonClient.GetAsync(checkUrl);
        }
        catch (HttpRequestException)
        {
            isUseful = false;
        }

        return isUseful;
    }

    /// <summary>
    /// 等待startSecond到endSecond秒后继续，文本控件showText提示正在倒计时
    /// </summary>
    /// <param name="startSecond"></param>
    /// <param name="endSecond"></param>
    public static async Task RandomTimeDelay(int startSecond, int endSecond)
    {
        //随机等待1-10s
        var randomSecond = new Random().Next(startSecond, endSecond);

        //倒计时
        await Task.Delay(randomSecond * 1000);
    }

    public static HttpClient CreateClient(Dictionary<string, string> headers)
    {
        if (headers == null || headers.Count == 0) return new HttpClient();

        var handler = new HttpClientHandler { UseCookies = false };
        var client = new HttpClient(handler);

        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        return client;
    }

    private static HttpClient _client;

    public static HttpClient CommonClient
    {
        get { return _client ??= NetworkHelper.CreateClient(new Dictionary<string, string> { { "user-agent", DbNetworkHelper.DownUserAgent } }); }
    }
}