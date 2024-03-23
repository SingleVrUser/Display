using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HttpHeaders = Display.Constants.HttpHeaders;

namespace Display.Helper.Data
{
    internal class OssSignHelper
    {
        private const string NewLineMarker = "\n";
        private const string OssPrefix = "x-oss-";
        private static readonly IList<string> ParametersToSign = new List<string> {
            "acl", "uploadId", "partNumber", "uploads", "cors", "logging",
            "website", "delete", "referer", "lifecycle", "security-token","append",
            "position", "x-oss-process", "restore", "bucketInfo", "stat", "symlink",
            "location", "qos", "policy", "tagging", "requestPayment", "x-oss-traffic-limit",
            "objectMeta", "encryption", "versioning", "versionId", "versions",
            "live", "status", "comp", "vod", "startTime", "endTime",
            "inventory","continuation-token","inventoryId",
            "callback", "callback-var","x-oss-request-payer",
            "worm","wormId","wormExtend","response-content-type",
            "response-content-language","response-expires","response-cache-control",
            "response-content-disposition","response-content-encoding"

        };

        private class KeyComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }

        public static string BuildCanonicalString(string method, string resourcePath, IDictionary<string, string> headers, IDictionary<string, string> parameters)
        {
            var canonicalString = new StringBuilder();

            canonicalString.Append(method).Append(NewLineMarker);

            IDictionary<string, string> headersToSign = new Dictionary<string, string>();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    var lowerKey = header.Key.ToLowerInvariant();

                    if (lowerKey == HttpHeaders.ContentType.ToLowerInvariant()
                        || lowerKey == HttpHeaders.ContentMd5.ToLowerInvariant()
                        || lowerKey == HttpHeaders.Date.ToLowerInvariant()
                        || lowerKey.StartsWith(OssPrefix))
                    {
                        headersToSign.Add(lowerKey, header.Value);
                    }
                }
            }

            if (!headersToSign.ContainsKey(HttpHeaders.ContentType.ToLowerInvariant()))
                headersToSign.Add(HttpHeaders.ContentType.ToLowerInvariant(), "");
            if (!headersToSign.ContainsKey(HttpHeaders.ContentMd5.ToLowerInvariant()))
                headersToSign.Add(HttpHeaders.ContentMd5.ToLowerInvariant(), "");

            // Add all headers to sign into canonical string, 
            // note that these headers should be ordered before adding.
            var sortedHeaders = new List<string>(headersToSign.Keys);
            sortedHeaders.Sort(new KeyComparer());
            foreach (var key in sortedHeaders)
            {
                var value = headersToSign[key];
                if (key.StartsWith(OssPrefix))
                    canonicalString.Append(key).Append(':').Append(value);
                else
                    canonicalString.Append(value);

                canonicalString.Append(NewLineMarker);
            }

            // Add canonical resource
            canonicalString.Append(BuildCanonicalizedResource(resourcePath, parameters));

            return canonicalString.ToString();
        }

        private static string BuildCanonicalizedResource(string resourcePath,
            IDictionary<string, string> parameters)
        {
            var canonicalizedResource = new StringBuilder();

            canonicalizedResource.Append(resourcePath);

            if (parameters == null) return canonicalizedResource.ToString();

            var parameterNames = new List<string>(parameters.Keys);
            parameterNames.Sort();

            var separator = '?';
            foreach (var paramName in parameterNames)
            {
                if (!ParametersToSign.Contains(paramName))
                    continue;

                canonicalizedResource.Append(separator);
                canonicalizedResource.Append(paramName);
                var paramValue = parameters[paramName];
                if (!string.IsNullOrEmpty(paramValue))
                    canonicalizedResource.Append("=").Append(paramValue);

                separator = '&';
            }

            return canonicalizedResource.ToString();
        }

        public static string GetAuthorization(string accessKeyId, string accessKeySecret, string httpMethod, string buckName,
            string objectName, IDictionary<string, string> headers, IDictionary<string, string> parameters)
        {
            var resourcePath = $"/{buckName}/{objectName}";

            var canonicalString = BuildCanonicalString(httpMethod, resourcePath, headers, parameters);

            var signature = ComputeSignature(accessKeySecret, canonicalString);

            return "OSS " + accessKeyId + ":" + signature;
        }

        private static string ComputeSignature(string key, string data)
        {
            var encode = Encoding.UTF8;
            using var algorithm = new HMACSHA1();
            algorithm.Key = encode.GetBytes(key.ToCharArray());

            return Convert.ToBase64String(
                algorithm.ComputeHash(encode.GetBytes(data.ToCharArray())));
        }

        internal static IDictionary<string, string> GetParametersFromSignedUrl(Uri signedUrl)
        {
            var parameters = new Dictionary<string, string>();
            var query = signedUrl.Query;
            var index = 0;
            if (query.Length > 0 && query[0] == '?')
            {
                index = 1;
            }

            var array = query[index..].Split('&');

            foreach (var i in array)
            {
                var param = i.Split('=');
                parameters.Add(HttpUtility.UrlDecode(param[0]), param.Length == 2 ? HttpUtility.UrlDecode(param[1]) : "");
            }

            return parameters;
        }

    }
}
