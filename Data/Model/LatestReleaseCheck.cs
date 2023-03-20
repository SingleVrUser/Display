

using System;
using System.Linq;
using static Data.Model.GitHubInfo;

namespace Data.Model;

public class LatestReleaseCheck
{
    public LatestReleaseCheck(ReleaseInfo releastInfo)
    {
        this.ReleastInfo = releastInfo;
        this.LatestVersion = releastInfo.TagName.ToLower().Replace("v", "");
        this.CurrentVersion = AppInfo.GetPackageVersion();
        this.CurrentArchitecture = AppInfo.GetPackageArchitecture();
        this.AppAsset = releastInfo.assets.Where(asset => asset.name.Contains(CurrentArchitecture)).FirstOrDefault();
        this.MoreInfoUrl = releastInfo.html_url;
        this.CanUpdate = CurrentVersion != LatestVersion && AppAsset != null;
        this.UpdateContent = releastInfo.UpdateContent;
        this.PublishedTime = releastInfo.PublishedTime;
    }

    /// <summary>
    /// Release信息
    /// </summary>
    public ReleaseInfo ReleastInfo { get; set; }

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
    public Asset AppAsset { get; set; }

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
