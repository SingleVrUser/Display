

using System;
using System.Linq;
using Display.Data;

namespace Display.Models;

public class LatestReleaseCheck
{
    public LatestReleaseCheck(GitHubInfo.ReleaseInfo releaseInfo)
    {
        this.ReleaseInfo = releaseInfo;
        this.LatestVersion = releaseInfo.TagName.ToLower().Replace("v", "");
        this.CurrentVersion = AppInfo.GetPackageVersion();
        this.CurrentArchitecture = AppInfo.GetPackageArchitecture();
        this.AppAsset = releaseInfo.assets.FirstOrDefault(asset => asset.name.Contains(CurrentArchitecture));
        this.MoreInfoUrl = releaseInfo.html_url;
        this.CanUpdate = CurrentVersion != LatestVersion && AppAsset != null;
        this.UpdateContent = releaseInfo.UpdateContent;
        this.PublishedTime = releaseInfo.PublishedTime;
    }

    /// <summary>
    /// Release信息
    /// </summary>
    public GitHubInfo.ReleaseInfo ReleaseInfo { get; set; }

    /// <summary>
    /// 最新版本
    /// </summary>
    public string LatestVersion { get; set; }

    /// <summary>
    /// 当前版本
    /// </summary>
    public string CurrentVersion { get; set; }

    /// <summary>
    /// 当前框架
    /// </summary>
    public string CurrentArchitecture { get; set; }

    /// <summary>
    /// 对应框架的Asset
    /// </summary>
    public GitHubInfo.Asset AppAsset { get; set; }

    /// <summary>
    /// 详情信息地址
    /// </summary>
    public string MoreInfoUrl { get; set; }

    /// <summary>
    /// 能否升级（当前版本号不等于最新版本号，拥有对应框架的下载包）
    /// </summary>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PublishedTime { get; set; }

    /// <summary>
    /// 升级内容
    /// </summary>
    public string UpdateContent { get; set; }
}
