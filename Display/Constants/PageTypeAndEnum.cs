using Display.Views;
using Display.Views.More;
using Display.Views.OfflineDown;
using Display.Views.Settings;
using Display.Views.Tasks;
using System;
using System.Collections.Generic;
using Display.Models.Enums;

namespace Display.Constants;

internal class PageTypeAndEnum
{
    public static Dictionary<NavigationViewItemEnum, Type> PageTypeAndEnumDict = new()
    {
        //菜单栏
        { NavigationViewItemEnum.HomePage, typeof(HomePage) },
        { NavigationViewItemEnum.VideoViewPage, typeof(VideoViewPage) },
        { NavigationViewItemEnum.ActorPage, typeof(ActorsPage) },
        { NavigationViewItemEnum.MorePage, typeof(MorePage) },
        { NavigationViewItemEnum.SettingPage, typeof(Views.Settings.MainPage) },
        { NavigationViewItemEnum.DownPage, typeof(OfflineDownPage) },
        { NavigationViewItemEnum.TaskPage, typeof(Views.Tasks.MainPage) },

        //后台任务
        { NavigationViewItemEnum.UploadTask, typeof(UploadTaskPage) },
        { NavigationViewItemEnum.SpiderTask, typeof(SpiderTaskPage) },

        //更多页
        { NavigationViewItemEnum.SpiderPage,typeof(Views.SpiderVideoInfo.MainPage) },
        { NavigationViewItemEnum.CalculateSha1Page,typeof(CalculateLocalFileSha1) },
        { NavigationViewItemEnum.BrowserPage,typeof(BrowserPage) },
        { NavigationViewItemEnum.FilePage,typeof(Views.More.DatumList.MainPage) },
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