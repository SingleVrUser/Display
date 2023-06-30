using Display.Helper;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Extensions
{
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

            try
            {
                var contentAsString = await response.Content.ReadAsStringAsync(token);

                Debug.WriteLine($"服务器返回的结果为:{contentAsString}");

                if (contentAsString is T value) return value;

                return contentAsString == "[]" ? defaultValue : JsonConvert.DeserializeObject<T>(contentAsString);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"发生错误{e.Message}");
                Toast.TryToast("格式异常", $"{nameof(T)}转换异常", e.Message);

                return defaultValue;
            }
        }

        public static async Task<T> SendAsync<T>(this HttpClient client, HttpRequestMessage request, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
        {
            return await SendAsync(client, request, CancellationToken.None, completionOption, defaultValue);
        }
        public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, CancellationToken token, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            return await SendAsync(client, request, token, completionOption, defaultValue);
        }

        public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead, T defaultValue = default)
        {
            return await SendAsync(client, method, url, CancellationToken.None, content, completionOption, defaultValue);
        }

    }
}
