using Display.Helper.Notifications;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Extensions;

internal static class HttpClientExtension
{

    public static async Task<T> SendAsync<T>(this HttpClient client, HttpRequestMessage request,
        CancellationToken token, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, completionOption, token);
        }
        catch (AggregateException e)
        {
            Toast.TryToast("网络异常", e.Message);
            return defaultValue;
        }
        catch (HttpRequestException e)
        {
            Toast.TryToast("网络异常", e.Message);
            return defaultValue;
        }
        catch (Exception e)
        {
            Toast.TryToast("网络异常", e.Message);
            return defaultValue;
        }

        if (!response.IsSuccessStatusCode) return defaultValue;

        string contentAsString = null;
        try
        {
            contentAsString = await response.Content.ReadAsStringAsync(token);

            if (contentAsString is T value) return value;

            if (contentAsString.Contains("[]"))
            {
                contentAsString = contentAsString.Replace("[]", "null");
            }

            return contentAsString == "null" ? defaultValue : JsonConvert.DeserializeObject<T>(contentAsString);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"结果转换失败：{contentAsString}");
            Toast.TryToast("格式异常", $"{typeof(T).Name}转换异常", e.Message);

            return defaultValue;
        }
    }

    public static async Task<T> SendAsync<T>(this HttpClient client, HttpRequestMessage request, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
    {
        return await SendAsync(client, request, CancellationToken.None, completionOption, defaultValue);
    }

    public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, CancellationToken token, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Content = content;

        return await SendAsync(client, request, token, completionOption, defaultValue);
    }

    public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
    {
        return await SendAsync(client, method, url, CancellationToken.None, content, completionOption, defaultValue);
    }

}