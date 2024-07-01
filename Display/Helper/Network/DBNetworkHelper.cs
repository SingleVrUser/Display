using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Display.Providers;

namespace Display.Helper.Network;

internal class DbNetworkHelper
{
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

    public static readonly string DownUserAgent = Constants.DefaultSettings.Network._115.DownUserAgent;

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
                    if (NetworkHelper.CommonClient.DefaultRequestHeaders.Contains(header.Key)) NetworkHelper.CommonClient.DefaultRequestHeaders.Remove(header.Key);

                    NetworkHelper.CommonClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // cloudflare检测
            if (!NetworkHelper.CommonClient.DefaultRequestHeaders.Contains("Cookie"))
            {
                NetworkHelper.CommonClient.DefaultRequestHeaders.Add("Cookie", "existmag=mag");
            }

            for (var i = 0; i < maxTryCount; i++)
            {
                try
                {
                    var fileBytes = await NetworkHelper.CommonClient.GetByteArrayAsync(url);

                    await File.WriteAllBytesAsync(localPath, fileBytes);
                    isSuccessDown = true;
                    break;
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"下载文件时发生错误：{ex.Message}");

                    // 禁止访问，直接停止
                    if(ex.StatusCode.Equals(HttpStatusCode.Forbidden)) break;
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