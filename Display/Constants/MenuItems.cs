using System;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.Views.Pages;
using Display.Views.Pages.More;
using Display.Views.Pages.OfflineDown;

namespace Display.Constants;

public static class MenuItems
{
    public static readonly MenuItem[] MainMenuItems =
    [
        new MenuItem(NavigationViewItemEnum.HomePage,"主页", "\xE10F", typeof(HomePage)),
        new MenuItem(NavigationViewItemEnum.VideoViewPage, "展示", "\xE8BA",typeof(VideoViewPage)),
        new MenuItem(NavigationViewItemEnum.ActorPage, "演员", "\xE77B", typeof(ActorsPage)),
        new MenuItem(NavigationViewItemEnum.MorePage, "其他", "\xE10C", typeof(MorePage))
    ];
    
    public static readonly MenuItem[] FootMenuItems =
    [
        new MenuItem(NavigationViewItemEnum.DownPage, "下载", "\xE118", typeof(OfflineDownPage))
        {
            CanSelected = false
        },
        new MenuItem(NavigationViewItemEnum.TaskPage, "任务", "\xE174", typeof(TaskPage))
        {
            CanSelected = false
        }
    ];
    
    public static readonly MoreMenuItem[] MoreMenuItems = [
        new MoreMenuItem(NavigationViewItemEnum.FilePage, "文件列表",  "115中的文件列表", "/Assets/Svg/file_alt_icon.svg", typeof(Views.Pages.More.DatumList.MainPage)),
        new MoreMenuItem(NavigationViewItemEnum.SpiderPage, "搜刮信息", "搜刮本地数据库中视频对应的信息", "/Assets/Svg/find_internet_magnifier_search_security_icon.svg", typeof(Views.Pages.SpiderVideoInfo.MainPage)),
        new MoreMenuItem(NavigationViewItemEnum.BrowserPage, "浏览器", "115网页版，并附加下载选项", "/Assets/Svg/explorer_internet_logo_logos_icon.svg", typeof(BrowserPage)),
        new MoreMenuItem(NavigationViewItemEnum.CalculateSha1Page, "计算Sha1", "计算本地文件的Sha1", "/Assets/Svg/accounting_banking_business_calculate_calculator_icon.svg", typeof(CalculateLocalFileSha1) )
        {
            Label = "测试中"
        }
    ];
}