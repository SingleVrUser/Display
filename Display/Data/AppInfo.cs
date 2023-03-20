
using Display.Models;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Display.Data;

public static class AppInfo
{
    private static readonly string LatestReleaseUrl = "https://api.github.com/repos/SingleVrUser/Display/releases/latest";

    /// <summary>
    /// 包版本（x.x.x）
    /// </summary>
    /// <returns></returns>
    public static string GetPackageVersion()
    {
        var appVersion = Package.Current.Id.Version;
        return $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}";
    }

    /// <summary>
    /// 包框架（x86或x64）
    /// </summary>
    /// <returns></returns>
    public static string GetPackageArchitecture()
    {
        return $"{Package.Current.Id.Architecture}".ToLower();
    }

    /// <summary>
    /// 获取github上最新的release信息
    /// </summary>
    /// <returns></returns>
    public static async Task<GitHubInfo.ReleaseInfo> GetGithubLatestReleaseAsync()
    {
        GitHubInfo.ReleaseInfo result = null;

        var Client = GetInfoFromNetwork.Client;

        //设置超时时间（5s）
        var option = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

        try
        {
            var req = await Client.GetAsync(LatestReleaseUrl, option);
            if (req.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await req.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<GitHubInfo.ReleaseInfo>(content);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取github上最新release时发生错误：{ex.Message}");
        }


        return result;
    }

    /// <summary>
    /// 检查是否是最新的（不包括忽略的版本）
    /// 考虑是否有对应框架的应用包
    /// </summary>
    public static async Task<LatestReleaseCheck> GetLatestReleaseCheck()
    {
        //从Github中获取最新信息
        var LatestReleaseInfo = await GetGithubLatestReleaseAsync();
        if (LatestReleaseInfo == null)
            return null;

        return new(LatestReleaseInfo);
    }
}
