using Display.Models.Dto.OneOneFive;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls.UserController;

public sealed partial class UserInfo : UserControl
{

    //status需要实时更新
    private static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(LoginStatus), typeof(UserInfo),
            new PropertyMetadata(LoginStatus.Update));

    public LoginStatus Status
    {
        get => (LoginStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    //userinfo需要实时更新
    private static readonly DependencyProperty UserinfoProperty =
        DependencyProperty.Register(nameof(Userinfo), typeof(UserData), typeof(UserInfo), null);

    public UserData Userinfo
    {
        get => (UserData)GetValue(UserinfoProperty);
        set => SetValue(UserinfoProperty, value);
    }


    public UserInfo()
    {
        this.InitializeComponent();
    }

    private Visibility IsShowVip(LoginStatus status)
    {
        return status == LoginStatus.None && UserNameTextBlock.Text == string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private Visibility IsShowUserInfo(LoginStatus status)
    {
        return status != LoginStatus.Login ? Visibility.Collapsed : Visibility.Visible;
    }

    private Visibility IsShowOtherInfo(LoginStatus status)
    {
        return status != LoginStatus.Login ? Visibility.Visible : Visibility.Collapsed;
    }

    private Visibility IsProcess(LoginStatus status)
    {
        return status == LoginStatus.NoLogin ? Visibility.Visible : Visibility.Collapsed;
    }

    private Visibility IsUpdate(LoginStatus status)
    {
        return status == LoginStatus.Update ? Visibility.Visible : Visibility.Collapsed;
    }

    public event RoutedEventHandler UpdateInfoClick;
    private void UpdateInfoButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateInfoClick?.Invoke(sender, e);
    }


    public event RoutedEventHandler LoginClick;
    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        LoginClick?.Invoke(sender, e);
    }


    public event RoutedEventHandler LogoutClick;
    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        LogoutClick?.Invoke(sender, e);
    }


    public enum LoginStatus
    {
        None,
        Update,
        NoLogin,
        Login
    }

}