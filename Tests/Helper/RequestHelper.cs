using Display.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Helper
{
    public class RequestHelper
    {
        private const string Cookie = UserInfo.Cookie;

        private static HttpClient? _uploadClient;
        public static HttpClient UploadClient
        {
            get
            {
                if (_uploadClient != null) return _uploadClient;

                var headers = new Dictionary<string, string>
                {
                    { "user-agent", Const.DefaultSettings.Network._115.UploadUserAgent },
                    { "Cookie", Cookie }
                };

                _uploadClient = GetInfoFromNetwork.CreateClient(headers);

                return _uploadClient;
            }
        }

        private static HttpClient? _downClient;
        public static HttpClient DownloadClient
        {
            get
            {
                if (_downClient != null) return _downClient;

                var headers = new Dictionary<string, string>
                {
                    { "user-agent", Const.DefaultSettings.Network._115.DownUserAgent },
                    { "Cookie", Cookie }
                };

                _downClient = GetInfoFromNetwork.CreateClient(headers);

                return _downClient;
            }
        }



    }
}
