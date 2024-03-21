using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Tasks;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UploadTaskPage : Page
{
    private readonly UploadViewModel _uploadVm = App.GetService<UploadViewModel>();

    public UploadTaskPage()
    {
        this.InitializeComponent();
    }
}