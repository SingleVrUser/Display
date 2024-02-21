using Display.Views.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.CustomWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow
    {
        //private AppWindow appWindow;
        //private WebApi webapi;
        //private DispatcherTimer _qrTimer;

        public static Microsoft.UI.Xaml.Window m_window;

        public LoginWindow()
        {
            this.InitializeComponent();

            m_window = this;
            //appWindow = App.getAppWindow(this);

            LoginPage loginPage = new LoginPage();
            this.Content = loginPage;

        }

    }
}
