using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Display.Models.Dto.Settings;
using Display.Models.Enums;
using Display.Views.Pages.Tasks;

namespace Display.ViewModels;

internal partial class TaskViewModel : ObservableObject
{
    public readonly MenuItem[] NavLinks =
    [
        new MenuItem(NavigationViewItemEnum.UploadTask, "上传", "\uF6FA", typeof(UploadTaskPage) ),
        new MenuItem(NavigationViewItemEnum.SpiderTask, "搜刮", "\uF6FA", typeof(SpiderTaskPage)),
    ];

    [ObservableProperty]
    private MenuItem _currentLink;

    public void SetCurrentLink(NavigationViewItemEnum pageEnum)
    {
        var link = NavLinks.FirstOrDefault(link => link.PageEnum == pageEnum);
        if (link is null) return;
        CurrentLink = link;
    }

}