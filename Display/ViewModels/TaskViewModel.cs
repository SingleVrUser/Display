using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Settings;
using System.Linq;
using Display.Models.Enums;

namespace Display.ViewModels;

internal partial class TaskViewModel : ObservableObject
{
    public MenuItem[] NavLinks =
    [
        new MenuItem("上传", "\uF6FA", NavigationViewItemEnum.UploadTask),
        new MenuItem("搜刮", "\uF6FA", NavigationViewItemEnum.SpiderTask),
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