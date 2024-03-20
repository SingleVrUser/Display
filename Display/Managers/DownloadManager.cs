using Display.Providers.Downloader;

namespace Display.Managers;

internal class DownloadManager
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