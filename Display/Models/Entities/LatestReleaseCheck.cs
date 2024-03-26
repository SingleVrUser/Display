using System;
using System.Linq;
using Display.Helper.Network;
using Display.Models.Dto;

namespace Display.Models.Entities;

public class LatestReleaseCheck
{
    public LatestReleaseCheck(GitHubInfo.ReleaseInfo releaseInfo)
    {
        ReleaseInfo = releaseInfo;
        LatestVersion = releaseInfo.TagName.ToLower().Replace("v", "");
        CurrentVersion = AppUpdateHelper.GetPackageVersion();
        CurrentArchitecture = AppUpdateHelper.GetPackageArchitecture();
        AppAsset = releaseInfo.assets.FirstOrDefault(asset => asset.name.Contains(CurrentArchitecture));
        MoreInfoUrl = releaseInfo.HtmlUrl;
        CanUpdate = CurrentVersion != LatestVersion && AppAsset != null;
        UpdateContent = releaseInfo.UpdateContent;
        PublishedTime = releaseInfo.PublishedTime;
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
