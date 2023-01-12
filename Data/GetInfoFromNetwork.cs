using Data.Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Ocr;

namespace Data
{
    public class GetInfoFromNetwork
    {
        public static bool IsJavDbCookieVisiable = true;

        private static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = CreateClient(new() { { "user-agent", DesktopUserAgent } });
                }

                return _client;
            }
            set=> _client = value;
        }

        private static HttpClient _clientWithJavDBCookie;
        public static HttpClient ClientWithJavDBCookie
        {
            get
            {
                if (_clientWithJavDBCookie == null)
                {
                    _clientWithJavDBCookie = CreateClient(new Dictionary<string, string>() {
                        {"cookie",AppSettings.javdb_Cookie },
                        {"user-agent" ,BrowserUserAgent}
                    });
                }

                return _clientWithJavDBCookie;
            }

            set => _clientWithJavDBCookie = value;
        }

        public static string BrowserUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 115Browser/8.3.0";
        public static string DesktopUserAgent = "Mozilla/5.0; Windows NT/10.0.19044; 115Desktop/2.0.1.7";
        public static string MediaElementUserAgent = "NSPlayer/12.00.22621.0963 WMFSDK/12.00.22621.0963";


        public GetInfoFromNetwork()
        {
        }

        public static HttpClient CreateClient(Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return new HttpClient();
            }

            var handler = new HttpClientHandler { UseCookies = false };
            var Client = new HttpClient(handler);

            foreach (var header in headers)
            {
                Client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            return Client;
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
        public async Task<bool> CheckUrlUseful(string checkUrl)
        {
            bool isUseful = true;
            HttpResponseMessage resp;
            try
            {
                resp = await Client.GetAsync(checkUrl);
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
            for (int i = 0; i < randomSecond; i++)
            {
                await Task.Delay(1000);
            }
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
        public static async Task<string> downloadFile(string url, string filePath, string fileName, bool isReplaceExistsImage = false, Dictionary<string, string> headers = null)
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

            string localPath = Path.Combine(filePath, localFilename);

            bool isSuccessDown = false;

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            //不存在
            if (!File.Exists(localPath) || isReplaceExistsImage)
            {
                int maxTryCount = 3;

                if(headers!= null)
                {
                    foreach(var header in headers)
                    {
                        if (Client.DefaultRequestHeaders.Contains(header.Key))
                            Client.DefaultRequestHeaders.Remove(header.Key);

                        Client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                for (int i = 0; i < maxTryCount; i++)
                {
                    try
                    {
                        byte[] fileBytes = await Client.GetByteArrayAsync(url);

                        File.WriteAllBytes(localPath, fileBytes);
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

            if (isSuccessDown)
                return localPath;
            else
                return null;

        }

    }
}
