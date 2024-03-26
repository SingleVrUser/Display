using Microsoft.UI.Xaml.Data;
using System;
using System.Text.RegularExpressions;

namespace Display.Converter;

public class AppUpdateContentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string content) return null;

        if (string.IsNullOrEmpty(content)) return null;

        //删除 by @xxx 
        var rgx = new Regex("by @.*? ", RegexOptions.IgnoreCase);
        var finallyContent = rgx.Replace(content, "");

        //http://……/pull/xx 替换为 #xx Markdown格式
        rgx = new Regex(@"https://.*?/pull/(\d+)", RegexOptions.IgnoreCase);
        finallyContent = rgx.Replace(finallyContent, "[#$1]($0)");


        //http://……/compare/xx 替换为 xx Markdown格式
        rgx = new Regex(@"https://.*?/compare/([v.0-9]+)", RegexOptions.IgnoreCase);
        finallyContent = rgx.Replace(finallyContent, "[$1]($0)");

        return finallyContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
