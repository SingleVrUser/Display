using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Settings;
using Display.Views.Tasks;

namespace Display.ViewModels;

internal partial class TaskViewModel : ObservableObject
{
    public NavLink[] NavLinks =
    [
        new NavLink { Label = "上传", Glyph = "\uF6FA", NavPageType = typeof(UploadTaskPage)},
        new NavLink { Label = "搜刮", Glyph = "\uF6FA", NavPageType = typeof(SpiderTaskPage)}
    ];

    [ObservableProperty]
    private NavLink _currentLink;

    public TaskViewModel()
    {
        _currentLink = NavLinks[^1];
    }
}