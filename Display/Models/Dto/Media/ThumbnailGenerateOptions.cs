using Display.Models.Media;

namespace Display.Models.Dto.Media;

public class ThumbnailGenerateOptions
{
    public int FrameCount = 10;

    public string SavePath;

    public string StringFormat;

    public UrlOptions UrlOptions;
}