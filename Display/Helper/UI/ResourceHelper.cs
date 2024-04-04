using Display.Models.Enums;
using Display.Models.Vo.OneOneFive;
using Microsoft.UI.Xaml;

namespace Display.Helper.UI;

internal static class ResourceHelper
{
    internal static bool TryGetResourceValue<T>(this FrameworkElement element, string key, out T result)
    {
        if (!element.Resources.TryGetValue(key, out var value)
            || value is not T tmpResult)
        {
            result = default;
            return false;
        }

        result = tmpResult;
        return true;
    }
    
    internal static string GetFileIconFromType(FileType fileType)
    {
        var iconUrl = Constants.FileType.UnknownSvgPath;

        switch (fileType)
        {
            case FileType.Folder:
                iconUrl = Constants.FileType.FolderSvgPath;
                break;
            case FileType.File:
                iconUrl = Constants.FileType.UnknownSvgPath;
                break;
        }

        return iconUrl;
    }
}