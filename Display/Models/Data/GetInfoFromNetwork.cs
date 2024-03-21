using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Display.Models.Data
{
    public class GetInfoFromNetwork
    {
        public static bool IsJavDbCookieVisible = true;

        public static bool IsX1080XCookieVisible = true;

        private static HttpClient _client;
        public static HttpClient CommonClient
        {
            get { return _client ??= CreateClient(new Dictionary<string, string> { { "user-agent", DownUserAgent } }); }
            set => _client = value;
        }

        private static HttpClient _clientWithJavDbCookie;
        public static HttpClient ClientWithJavDbCookie
        {
            get
            {
                if (_clientWithJavDbCookie != null) return _clientWithJavDbCookie;

                _clientWithJavDbCookie = CreateClient(new Dictionary<string, string>() {
                    {"cookie",AppSettings.JavDbCookie },
                    {"user-agent" ,DownUserAgent}
                });

                return _clientWithJavDbCookie;
            }

            set => _clientWithJavDbCookie = value;
        }

        public static string DownUserAgent = Constant.DefaultSettings.Network._115.DownUserAgent;

        //public static string BuilderMediaElementUserAgent()
        //{
        //    string build = Environment.OSVersion.Version.Build.ToString();
        //    string ubr = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "UBR", "1265").ToString();

        //    return $"NSPlayer/12.00.{build}.{ubr} WMFSDK/12.00.{build}.{ubr}";
        //}

        public static HttpClient CreateClient(Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return new HttpClient();
            }

            var handler = new HttpClientHandler { UseCookies = false };
            var client = new HttpClient(handler);

            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            return client;
        }

        public static string UrlCombine(string uri1, string uri2)
        {

            Uri baseUri = new Uri(uri1);
            Uri myUri = new Uri(baseUri, uri2);
            return myUri.ToString();
        }

        /// <summary>
        /// 检查是否能访问该网页
        /// </summary>
        /// <param Name="checkUrl"></param>
        /// <returns></returns>
        public static async Task<bool> CheckUrlUseful(string checkUrl)
        {
            bool isUseful = true;
            HttpResponseMessage resp;
            try
            {
                resp = await CommonClient.GetAsync(checkUrl);
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
        /// <param Name="startSecond"></param>
        /// <param Name="endSecond"></param>
        /// <param Name="showText"></param>
        public static async Task RandomTimeDelay(int startSecond, int endSecond)
        {
            //随机等待1-10s
            int randomSecond = new Random().Next(startSecond, endSecond);

            //倒计时
            await Task.Delay(randomSecond * 1000);
        }

        /// <summary>
        /// 下载文件，并返回文件路径
        /// </summary>
        /// <param Name="url">下载地址</param>
        /// <param Name="filePath">保存路径</param>
        /// <param Name="fileName">文件的名称</param>
        /// <param Name="isReplaceExistsImage">是否取代存在的文件，默认为否</param>
        /// <param Name="headers">需要的header，可选</param>
        /// <returns>下载后的文件路径</returns>
        public static async Task<string> DownloadFile(string url, string filePath, string fileName, bool isReplaceExistsImage = false, Dictionary<string, string> headers = null)
        {
            string localFilename;
            if (fileName.Contains("."))
            {
                localFilename = fileName;
            }
            else
            {
                localFilename = $"{fileName}{Path.GetExtension(url)}";

                if (localFilename.Contains("?"))
                {
                    localFilename = localFilename.Split("?")[0];
                }
            }

            var localPath = Path.Combine(filePath, localFilename);

            bool isSuccessDown = false;

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            //不存在
            if (!File.Exists(localPath) || isReplaceExistsImage)
            {
                var maxTryCount = 3;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (CommonClient.DefaultRequestHeaders.Contains(header.Key))
                            CommonClient.DefaultRequestHeaders.Remove(header.Key);

                        CommonClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                for (int i = 0; i < maxTryCount; i++)
                {
                    try
                    {
                        byte[] fileBytes = await CommonClient.GetByteArrayAsync(url);

                        await File.WriteAllBytesAsync(localPath, fileBytes);
                        isSuccessDown = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"下载文件时发生错误：{ex.Message}");
                    }
                }
            }
            //存在
            else
            {
                isSuccessDown = true;
            }

            return isSuccessDown ? localPath : null;
        }

    }
}
