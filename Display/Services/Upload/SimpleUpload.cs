using Display.Extensions;
using Display.Models.Upload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HttpHeaders = Display.Models.Data.Const.HttpHeaders;

namespace Display.Services.Upload
{
    internal class SimpleUpload : OssUploadBase
    {
        public SimpleUpload(HttpClient client,FileStream stream, OssToken ossToken, FastUploadResult fastUploadResult)
            : base(client,stream, ossToken, fastUploadResult)
        {
        }

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
}
