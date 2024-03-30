using Display.Models.Vo.Down;
using Display.Providers.Downloader;

namespace Display.Managers;

internal static class DownloadManager
{
    private static BaseDownloader[] _downloader;
    public static BaseDownloader[] Downloads
    {
        get
        {
            return _downloader ??= [
                new Bitcomet(),
                new Aria2()
            ];
        }
    }
}