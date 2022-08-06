using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckCookie : Page
    {
        private List<Cookie_key_value> CookieDict_List;

        public CheckCookie()
        {
            this.InitializeComponent();
        }

        public CheckCookie(string cookies)
        {
            this.InitializeComponent();

            CookieDict_List = FormatCookie(cookies);
        }

        private List<Cookie_key_value> FormatCookie(string cookie)
        {
            CookieDict_List = new List<Cookie_key_value>();

            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                var cookiesList = cookie.Split(';');
                int index = 0;
                foreach (var cookies in cookiesList)
                {
                    index++;
                    cookie = cookies;
                    var item = cookies.Split('=');
                    string key = item[0].Trim();
                    string value = item.Length > 1 ? item[1].Trim() : string.Empty;

                    CookieDict_List.Add(new Cookie_key_value() { index = index,  key = key, value = value });;
                }
            }
            return CookieDict_List;
        }

    }

    partial class Cookie_key_value
    {
        public int index;
        public string key;
        public string value;
    }
}
