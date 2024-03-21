using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Settings;
using Display.Views.Tasks;

namespace Display.ViewModels;

internal partial class TaskViewModel : ObservableObject
{
    public NavLink[] NavLinks =
    [
        new NavLink { Label = "上传", Glyph = "\uF6FA", NavPageType = typeof(UploadTaskPage)}
    ];

    private readonly UploadViewModel _uploadViewModel = App.GetService<UploadViewModel>();

    [ObservableProperty]
    private NavLink _currentLink;

    public TaskViewModel()
    {
        _currentLink = NavLinks[0];
    }
}