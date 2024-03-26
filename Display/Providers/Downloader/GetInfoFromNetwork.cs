using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Display.Helper.Network;

namespace Display.Providers.Downloader;

public class GetInfoFromNetwork
{
    public static bool IsJavDbCookieVisible = true;

    public static bool IsX1080XCookieVisible = true;

    private static HttpClient _client;
    public static HttpClient CommonClient
    {
        get { return _client ??= NetworkHelper.CreateClient(new Dictionary<string, string> { { "user-agent", DownUserAgent } }); }
        set => _client = value;
    }

    private static HttpClient _clientWithJavDbCookie;
    public static HttpClient ClientWithJavDbCookie
    {
        get
        {
            if (_clientWithJavDbCookie != null) return _clientWithJavDbCookie;

            _clientWithJavDbCookie = NetworkHelper.CreateClient(new Dictionary<string, string>() {
                {"cookie",AppSettings.JavDbCookie },
                {"user-agent" ,DownUserAgent}
            });

            return _clientWithJavDbCookie;
        }

        set => _clientWithJavDbCookie = value;
    }

    public static string DownUserAgent = Constants.DefaultSettings.Network._115.DownUserAgent;

    /// <summary>
    /// 下载文件，并返回文件路径
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="filePath">保存路径</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="isReplaceExistsImage">是否取代存在的文件，默认为否</param>
    /// <param name="headers">需要的header，可选</param>
    /// <returns>下载后的文件路径</returns>
    public static async Task<string> DownloadFile(string url, string filePath, string fileName, bool isReplaceExistsImage = false, Dictionary<string, string> headers = null)
    {
        string localFilename;
        if (fileName.Contains('.'))
        {
            localFilename = fileName;
        }
        else
        {
            localFilename = $"{fileName}{Path.GetExtension(url)}";

            if (localFilename.Contains('?'))
            {
                localFilename = localFilename.Split("?")[0];
            }
        }

        var localPath = Path.Combine(filePath, localFilename);

        var isSuccessDown = false;

        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

        //不存在
        if (!File.Exists(localPath) || isReplaceExistsImage)
        {
            const int maxTryCount = 3;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (CommonClient.DefaultRequestHeaders.Contains(header.Key))
                        CommonClient.DefaultRequestHeaders.Remove(header.Key);

                    CommonClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            for (var i = 0; i < maxTryCount; i++)
            {
                try
                {
                    var fileBytes = await CommonClient.GetByteArrayAsync(url);

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