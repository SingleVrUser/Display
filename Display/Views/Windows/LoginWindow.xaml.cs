using System;
using Display.Views.Pages.Settings.Account;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Windows;

public sealed partial class LoginWindow
{
    public LoginWindow(Page page)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        RootContent.Content = page;
    }

    public static void ShowLoginWindow(Action closeAction)
    {
        var loginPage = new LoginPage();
        var loginWindow = new LoginWindow(loginPage);

        loginPage.LoginCompletedAction = () =>
        {
            loginWindow.Close();
            closeAction.Invoke();
        };
        loginWindow.Activate();
    }


}