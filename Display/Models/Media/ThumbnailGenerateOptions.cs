using Display.Models.Image;

namespace Display.Models.Media;

public class ThumbnailGenerateOptions
{
    public int FrameCount = 10;

    public string SavePath;

    public string StringFormat;

    public UrlOptions UrlOptions;
}