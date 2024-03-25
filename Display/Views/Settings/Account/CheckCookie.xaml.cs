using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckCookie : Page
    {
        private readonly CookieKeyValue[] _cookieDictList;

        public CheckCookie(string cookies)
        {
            InitializeComponent();

            _cookieDictList = FormatCookie(cookies);
        }

        private CookieKeyValue[] FormatCookie(string cookie)
        {
            //cookie不为空且可用
            if (string.IsNullOrEmpty(cookie)) return [];

            var cookieList = cookie.Split(';');

            var result = new CookieKeyValue[cookieList.Length];
            for (var i = 0; i < cookieList.Length; i++)
            {
                var curCookie = cookieList[i];
                var item = curCookie.Split('=');

                var key = item[0].Trim();
                var value = item.Length > 1 ? item[1].Trim() : string.Empty;

                _cookieDictList[i] = new CookieKeyValue(i + 1, key, value);
            }

            return result;
        }

    }

    internal record CookieKeyValue(int Index, string Key, string Value);
}
