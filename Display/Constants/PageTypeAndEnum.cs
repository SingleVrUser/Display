using System;
using System.Collections.Generic;
using Display.Models.Enums;
using Display.Views.Pages;
using Display.Views.Pages.More;
using Display.Views.Pages.OfflineDown;
using Display.Views.Pages.Settings;
using Display.Views.Pages.Tasks;
using MainPage = Display.Views.Pages.Settings.MainPage;

namespace Display.Constants;

internal static class PageTypeAndEnum
{
    public static Dictionary<NavigationViewItemEnum, Type> PageTypeAndEnumDict = new()
    {
        //菜单栏
        { NavigationViewItemEnum.HomePage, typeof(HomePage) },
        { NavigationViewItemEnum.VideoViewPage, typeof(VideoViewPage) },
        { NavigationViewItemEnum.ActorPage, typeof(ActorsPage) },
        { NavigationViewItemEnum.MorePage, typeof(MorePage) },
        { NavigationViewItemEnum.SettingPage, typeof(MainPage) },
        { NavigationViewItemEnum.DownPage, typeof(OfflineDownPage) },
        { NavigationViewItemEnum.TaskPage, typeof(Views.Pages.Tasks.MainPage) },

        //后台任务
        { NavigationViewItemEnum.UploadTask, typeof(UploadTaskPage) },
        { NavigationViewItemEnum.SpiderTask, typeof(SpiderTaskPage) },

        //更多页
        { NavigationViewItemEnum.SpiderPage,typeof(Views.Pages.SpiderVideoInfo.MainPage) },
        { NavigationViewItemEnum.CalculateSha1Page,typeof(CalculateLocalFileSha1) },
        { NavigationViewItemEnum.BrowserPage,typeof(BrowserPage) },
        { NavigationViewItemEnum.FilePage,typeof(Views.Pages.More.DatumList.MainPage) },
        { NavigationViewItemEnum.ActorCoverPage,typeof(AddActorCover) },
        { NavigationViewItemEnum.ThumbnailPage,typeof(GetThumbnail) },
        
        //设置
        { NavigationViewItemEnum.AccountSetting, typeof(AccountPage) },
        { NavigationViewItemEnum.CommonSetting, typeof(CommonPage) },
        { NavigationViewItemEnum.UIShowSetting, typeof(UIShowPage) },
        { NavigationViewItemEnum.PlaySetting, typeof(PlayPage) },
        { NavigationViewItemEnum.SpiderSetting, typeof(SpiderPage) },
        { NavigationViewItemEnum.SearchSetting, typeof(SearchPage) },
        { NavigationViewItemEnum.StorageSetting, typeof(StoragePage) },
        { NavigationViewItemEnum.DownSetting, typeof(DownPage) }
    };
}