using Display.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Display.Helper;
using HttpHeaders = Display.Data.Const.HttpHeaders;

namespace Display.Services.Upload
{
    internal class OssUploadBase
    {
        protected readonly Uri EndpointUri;
        protected readonly HttpClient Client;
        protected readonly string AccessKeyId;
        protected readonly string AccessKeySecret;
        protected readonly string CallbackBase64;
        protected readonly string CallbackVarBase64;
        protected readonly string BucketName;
        protected readonly string ObjectName;
        protected readonly string SecurityToken;
        protected readonly string BaseUrl;

        private static string Data => DateTime.UtcNow.ToString(@"ddd, dd MMM yyyy HH:mm:ss G\MT",
            CultureInfo.InvariantCulture);

        protected OssUploadBase(HttpClient client, string endpoint, string accessKeyId, string accessKeySecret, string securityToken, string bucketName, string objectName, string callbackBase64, string callbackVarBase64)
        {
            Client = client;
            EndpointUri = new Uri(endpoint);
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            SecurityToken = securityToken;

            CallbackBase64 = callbackBase64;
            CallbackVarBase64 = callbackVarBase64;
            BucketName = bucketName;
            ObjectName = objectName;

            BaseUrl = $"http://{BucketName}.{EndpointUri.Authority}/{ObjectName}";
        }

        protected OssUploadBase(HttpClient client, string endpoint, OssToken ossToken, Upload115Result upload115Result)
        {
            Client = client;
            EndpointUri = new Uri(endpoint);

            AccessKeyId = ossToken.AccessKeyId;
            AccessKeySecret = ossToken.AccessKeySecret;
            SecurityToken = ossToken.SecurityToken;

            CallbackBase64 = upload115Result.callback.callback;
            CallbackVarBase64 = upload115Result.callback.callback_var;
            BucketName = upload115Result.bucket;
            ObjectName = upload115Result.Object;

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
    }
}
