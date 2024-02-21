using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using HttpHeaders = Display.Models.Data.Const.HttpHeaders;
using System.Threading;
using System.IO;
using Display.Models.Upload;
using Display.Helper.Data;

namespace Display.Services.Upload
{
    internal abstract class OssUploadBase:IDisposable
    {
        private const string Endpoint = "http://oss-cn-shenzhen.aliyuncs.com";

        private static string Data => DateTime.UtcNow.ToString(@"ddd, dd MMM yyyy HH:mm:ss G\MT",
            CultureInfo.InvariantCulture);

        protected readonly HttpClient Client;
        protected readonly FileStream Stream;
        protected readonly Uri EndpointUri;
        protected readonly long FileSize;
        protected readonly string AccessKeyId;
        protected readonly string AccessKeySecret;
        protected readonly string CallbackBase64;
        protected readonly string CallbackVarBase64;
        protected readonly string BucketName;
        protected readonly string ObjectName;
        protected readonly string SecurityToken;
        protected readonly string BaseUrl;

        protected readonly CancellationTokenSource Cts = new();
        protected CancellationToken Token => Cts.Token;

        protected OssUploadBase(HttpClient client, FileStream stream, OssToken ossToken, FastUploadResult fastUploadResult)
        {
            Client = client;
            Stream = stream;
            FileSize = stream.Length;

            EndpointUri = new Uri(Endpoint);
            AccessKeyId = ossToken.AccessKeyId;
            AccessKeySecret = ossToken.AccessKeySecret;
            SecurityToken = ossToken.SecurityToken;

            CallbackBase64 = fastUploadResult.callback.callback;
            CallbackVarBase64 = fastUploadResult.callback.callback_var;
            BucketName = fastUploadResult.bucket;
            ObjectName = fastUploadResult.Object;

            BaseUrl = $"http://{BucketName}.{EndpointUri.Authority}/{ObjectName}";
        }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string url, IDictionary<string, string> headers = null, HttpContent content = null)
        {
            var uri = new Uri(url);

            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = content
            };

            headers ??= new Dictionary<string, string>();
            headers[HttpHeaders.SecurityToken] = SecurityToken;
            headers[HttpHeaders.Date] = Data;
            headers[HttpHeaders.Authorization] = OssSignHelper.GetAuthorization(AccessKeyId, AccessKeySecret, method.ToString(),
                BucketName, ObjectName, headers, OssSignHelper.GetParametersFromSignedUrl(uri));
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return request;
        }

        public abstract Task<OssUploadResult> Start();

        public abstract void Stop();

        public void Dispose()
        {
            Cts.Cancel();
            Cts.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
