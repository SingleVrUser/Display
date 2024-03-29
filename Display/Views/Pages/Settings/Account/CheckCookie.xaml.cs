using Display.Helper.Data;
using Display.Models.Data;

namespace Display.Views.Settings.Account;

public sealed partial class CheckCookie
{
    private readonly CookieKeyValue[] _cookieKeyValueArray;

    public CheckCookie(string cookies)
    {
        InitializeComponent();

        _cookieKeyValueArray = CookieHelper.FormatCookieArray(cookies);
    }
}