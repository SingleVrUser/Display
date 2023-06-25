using System;
using Display.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using HttpHeaders = Display.Data.Const.HttpHeaders;
using Display.Extensions;

namespace Display.Services.Upload
{
    internal class SimpleUpload : OssUploadBase
    {
        public SimpleUpload(HttpClient client, string endpoint, string accessKeyId, string accessKeySecret, string securityToken, string bucketName, string objectName, string callback, string callbackVarBase64)
            : base(client, endpoint, accessKeyId, accessKeySecret, securityToken, bucketName, objectName, callback, callbackVarBase64)
        {
        }

        public SimpleUpload(HttpClient client, string endpoint, OssToken ossToken, Upload115Result upload115Result)
            : base(client, endpoint, ossToken, upload115Result)
        {
        }

        public async Task<bool> Start(FileStream stream, CancellationToken token)
        {
            const string contentType = "application/octet-stream";
            var headers = new Dictionary<string, string>
            {
                [HttpHeaders.Callback] = CallbackBase64,
                [HttpHeaders.CallbackVar] = CallbackVarBase64,
                [HttpHeaders.ContentType] = contentType
            };

            stream.Seek(0, SeekOrigin.Begin);
            var httpContent = new StreamContent(stream)
            {
                Headers = { ContentType = MediaTypeHeaderValue.Parse(contentType) }
            };
            var request = CreateRequest(HttpMethod.Put, BaseUrl, headers, httpContent);

            try
            {
                var content = await Client.SendAsync<string>(request, token);

                Debug.WriteLine($"上传结果：{content}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"上传出错：{ex.Message}");
            }

            return false;
        }

    }
}
