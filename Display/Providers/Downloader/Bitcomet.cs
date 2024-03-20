using System;
using Display.Models.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Windows.System;

namespace Display.Providers.Downloader;

internal class Bitcomet : BaseDownloader
{
    public override string Name => "Bitcomet";
    public override string Description => "调用 比特彗星 的“网页版远程下载”";
    public override string ApiPlaceholder => "http://用户名:密码@IP地址:监听端口，例:http://admin:123@127.0.0.1:1235";

    public override string SavePath { get; set; } = AppSettings.BitCometSavePath;

    public override string ApiText { get; set; } = ReadApiSetting(AppSettings.BitCometSettings);

    protected override async void CheckApiOk(DownApiSettings curSetting)
    {
        var isOk = false;

        var handler = new HttpClientHandler
        {
            UseDefaultCredentials = true,
            Credentials = new NetworkCredential(curSetting.UserName, curSetting.Password)
        };

        using HttpClient httpClient = new(handler);

        try
        {
            var rep = await httpClient.GetAsync(curSetting.ApiUrl + "/panel/task_add_httpftp");

            if (rep.IsSuccessStatusCode)
            {
                var content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOk = true;
            }
            else
            {
                if (rep.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ShowError("测试连接", "认证失败,请检查用户名和密码");
                }
            }
        }
        catch (UriFormatException)
        {
            ShowError("测试连接", "访问失败,请检查地址和端口");
        }
        catch (Exception ex)
        {
            ShowError("测试连接", $"出错了:{ex.Message}");
        }

        if (isOk) ShowSuccess("通信正常");
    }
    protected override DownApiSettings MatchApiSetting()
    {
        var isMatch = Regex.Match(ApiText, "^(https?://)(\\w+):(\\w+)@([\\w.]+:(\\d+))/?$");

        if (isMatch.Success) return new DownApiSettings
        {
            ApiUrl = $"{isMatch.Groups[1].Value}{isMatch.Groups[4].Value}",
            UserName = isMatch.Groups[2].Value,
            Password = isMatch.Groups[3].Value
        };
        return null;
    }

    protected override void SetSavePath(string newPath)
        => AppSettings.BitCometSavePath = newPath;

    protected override void SetApiSetting(DownApiSettings settings)
        => AppSettings.BitCometSettings = settings;

}