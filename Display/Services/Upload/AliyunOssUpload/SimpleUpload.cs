using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Display.Extensions;
using Display.Models.Api.OneOneFive.Upload;
using Display.Models.Vo;
using HttpHeaders = Display.Constants.HttpHeaders;

namespace Display.Services.Upload.AliyunOssUpload;

/// <summary>
/// 简单上传
/// </summary>
/// <param name="client"></param>
/// <param name="stream"></param>
/// <param name="ossToken"></param>
/// <param name="fastUploadResult"></param>
/// <param name="token"></param>
internal class SimpleUpload(
    HttpClient client,
    FileStream stream,
    OssToken ossToken,
    FastUploadResult fastUploadResult,
    CancellationToken token)
    : BaseOssUpload(client, stream, ossToken, fastUploadResult, token)
{
    public override async Task<OssUploadResult> Start()
    {
        const string contentType = "application/octet-stream";
        var headers = new Dictionary<string, string>
        {
            [HttpHeaders.Callback] = CallbackBase64,
            [HttpHeaders.CallbackVar] = CallbackVarBase64,
            [HttpHeaders.ContentType] = contentType
        };

        Stream.Seek(0, SeekOrigin.Begin);
        var httpContent = new StreamContent(Stream)
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(contentType) }
        };
        var request = CreateRequest(HttpMethod.Put, BaseUrl, headers, httpContent);

        OssUploadResult result = null;
        try
        {
            result = await Client.SendAsync<OssUploadResult>(request, Token);

            Debug.WriteLine($"上传结果：{result}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"上传出错：{ex.Message}");
        }

        Dispose();
        return result;
    }

    public override void Stop()
    {
        Dispose();
    }
}