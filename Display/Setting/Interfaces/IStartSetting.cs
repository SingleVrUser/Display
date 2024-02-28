namespace Display.Setting.Interfaces;

internal interface IStartSetting
{
    /// <summary>
    /// 是否左侧导航是否展开
    /// </summary>
    public bool IsNavigationViewPaneOpen { get; set; }

    /// <summary>
    /// 是否检查更新
    /// </summary>
    public bool IsCheckUpdate { get; set; }

    /// <summary>
    /// 应用的启动页面
    /// </summary>
    public int StartPageIndex { get; set; }
}