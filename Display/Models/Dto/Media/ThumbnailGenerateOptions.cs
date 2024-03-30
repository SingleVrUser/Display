namespace Display.Models.Dto.Media;

public class ThumbnailGenerateOptions
{
    public readonly int FrameCount = 10;

    public string SavePath;

    public string StringFormat;

    public UrlOptions UrlOptions;
}