using Display.ViewModels;

namespace Display.Views.Pages.Tasks;

public sealed partial class SpiderTaskPage
{
    private readonly SpiderTaskViewModel _viewModel = App.GetService<SpiderTaskViewModel>();

    public SpiderTaskPage()
    {
        InitializeComponent();
    }
}