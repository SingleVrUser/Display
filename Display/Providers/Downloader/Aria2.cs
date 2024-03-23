using System;
using System.Net;
using Display.Models.Data;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Display.Models.Settings.Options;

namespace Display.Providers.Downloader;

internal class Aria2: BaseDownloader
{
    public override string Name => "Aria2";

    public override string Description => "调用 Aria2 的 RPC";

    public override string ApiPlaceholder => "http://token:{secret}@{IP地址}:{监听端口}/jsonrpc";

    public override string SavePath { get; set; } = AppSettings.Aria2SavePath;
    protected override void SetSavePath(string newPath)
        => AppSettings.Aria2SavePath = newPath;

    protected override void SetApiSetting(DownApiSettings settings)
        => AppSettings.Aria2Settings = settings;

    protected override async void CheckApiOk(DownApiSettings curSetting)
    {
        var isOk = false;

        Aria2Request request = new()
        {
            jsonrpc = "2.0",
            method = "aria2.getVersion",
            id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            _params = [curSetting.Password]
        };

        var myContent = JsonConvert.SerializeObject(request);

        //myContent = myContent.Replace("_params", "params");

        using var handler = new SocketsHttpHandler();
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(1);

        using var httpClient = new HttpClient(handler);
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        var stringContent = new StringContent(myContent, Encoding.UTF8, "application/json");

        try
        {
            var rep = await httpClient.PostAsync(curSetting.ApiUrl, stringContent);

            if (rep.IsSuccessStatusCode)
            {
                var content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOk = true;
            }
            else
            {
                switch (rep.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        ShowWarn("测试连接", "请求失败，请检查Secret");
                        break;
                    case HttpStatusCode.NotFound:
                        ShowWarn("测试连接", "请求地址有误，请检查 地址是否以 \"/jsonrpc\" 结尾");
                        break;
                    case HttpStatusCode.BadGateway:
                        ShowWarn("测试连接", "端口有误");
                        break;
                }
            }
        }
        catch (HttpRequestException)
        {
            ShowWarn("测试连接", "请求失败，请检查地址和端口");
        }
        catch (TaskCanceledException)
        {
            ShowWarn("测试连接", "请求超时，请检查地址");
        }

        if (isOk) ShowSuccess("通信正常");
    }

    public override string ApiText { get; set; } = ReadApiSetting(AppSettings.Aria2Settings);

    protected override DownApiSettings MatchApiSetting()
    {
        var isMatch = Regex.Match(ApiText, "^https?://(\\w+:\\w+)@[\\w.]+(:\\d+)?/jsonrpc$");

        if (!isMatch.Success)
        {
            return null;
        }

        return new DownApiSettings
        {
            ApiUrl = isMatch.Value,
            Password = isMatch.Groups[1].Value
        };
    }



}