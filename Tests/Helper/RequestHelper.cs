using Display.Helper.Network;

namespace Tests.Helper;

public static class RequestHelper
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
                { "user-agent", Display.Constants.DefaultSettings.Network._115.UploadUserAgent },
                { "Cookie", Cookie }
            };

            _uploadClient = NetworkHelper.CreateClient(headers);

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
                { "user-agent", Display.Constants.DefaultSettings.Network._115.DownUserAgent },
                { "Cookie", Cookie }
            };

            _downClient = NetworkHelper.CreateClient(headers);

            return _downClient;
        }
    }



}