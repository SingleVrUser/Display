using Display.Providers.Spider;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Display.Models.Settings.Options;

internal class Spider : INotifyPropertyChanged
{
    public BaseSpider Instance { get; set; }

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

    public Spider(BaseSpider baseInstance)
    {
        Instance = baseInstance;

        Cookie = baseInstance.Cookie;
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