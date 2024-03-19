using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Display.Helper.Network.Spider;
using Microsoft.UI.Xaml;

namespace Display.Models.Settings.Options;

internal class Spider : INotifyPropertyChanged
{
    public InfoSpider Instance { get; set; }

    private string _cookie;

    public string Cookie
    {
        get => _cookie;
        set
        {
            if (_cookie == value) return;
            _cookie = value;
            OnPropertyChanged();
        }
    }

    public Action<string> SaveCookieAction { get; set; }

    public Spider(InfoSpider infoInstance)
    {
        Instance = infoInstance;

        Cookie = infoInstance.Cookie;
    }

    public void UpdateCookie(object sender, RoutedEventArgs e)
    {
        SaveCookieAction?.Invoke(Cookie);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}