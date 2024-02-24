namespace Display.Models.Data.Enums;

public enum PlayerType
{
    WebView = 0,
    PotPlayer = 1,
    Mpv = 2,
    Vlc = 3,
    MediaElement = 4,
    None = -1
}

public enum PlayQuality
{
    M3U8 = 0,
    Origin = 1
}

public enum ThumbnailOrigin { Local = 0, Web = 1 }

public enum SavePathEnum
{
    Data, CoverImage, ActorImage, Thumbnail, Subtitles
}