using System.Collections.Generic;
using Display.Models.Data;
using SharpCompress;

namespace Display.Helper.Data;

internal static class CookieHelper
{
    internal static IEnumerable<CookieKeyValue> ProductCookieKeyValue(string cookie)
    {
        if (string.IsNullOrEmpty(cookie)) yield break;

        var cookieList = cookie.Split(';');

        for (var i = 0; i < cookieList.Length; i++)
        {
            var curCookie = cookieList[i];
            var item = curCookie.Split('=');

            var key = item[0].Trim();
            var value = item.Length > 1 ? item[1].Trim() : string.Empty;

            yield return new CookieKeyValue(i + 1, key, value);
        }
    }

    internal static CookieKeyValue[] FormatCookieArray(string cookie)
    {
        List<CookieKeyValue> result = [];
        ProductCookieKeyValue(cookie).ForEach(result.Add);
        return [.. result];
    }
}

