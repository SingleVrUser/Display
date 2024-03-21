using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;


namespace Display.Views.Tasks;

public sealed partial class SpiderTaskPage : Page
{
    private readonly SpiderTaskViewModel _viewModel = App.GetService<SpiderTaskViewModel>();

    public SpiderTaskPage()
    {
        this.InitializeComponent();
    }
}