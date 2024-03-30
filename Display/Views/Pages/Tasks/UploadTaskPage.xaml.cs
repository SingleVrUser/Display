using Display.ViewModels;

namespace Display.Views.Pages.Tasks;

public sealed partial class UploadTaskPage
{
    private readonly UploadViewModel _uploadVm = App.GetService<UploadViewModel>();

    public UploadTaskPage()
    {
        InitializeComponent();
    }
}