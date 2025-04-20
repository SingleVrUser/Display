using System.Collections.Generic;

namespace Display.Constants;

public static class FileType
{
    public const string MsUri = "ms-appx:///";

    private const string AssetsBasePath = "Assets/";

    /// <summary>
    /// ms-appx:///Assets/
    /// </summary>
    private const string AssetsFullBasePath = MsUri + AssetsBasePath;

    /// <summary>
    /// Assets/115/file_type/
    /// </summary>
    public const string FileTypeBasePath = AssetsBasePath + "115/file_type/";

    /// <summary>
    /// ms-appx:///Assets/115/file_type/
    /// </summary>
    public const string FileTypeFullBasePath = MsUri + FileTypeBasePath;

    /// <summary>
    /// ms-appx:///Assets/NoPicture.jpg
    /// </summary>
    public const string NoPicturePath = AssetsFullBasePath + "NoPicture.jpg";

    public const string FailPicturePath = AssetsFullBasePath + "Fail.jpg";

    /// <summary>
    /// ms-appx:///Assets/115/file_type/folder/folder.svg
    /// </summary>
    public const string FolderSvgPath = FileTypeFullBasePath + "folder/folder.svg";

    /// <summary>
    /// ms-appx:///Assets/115/file_type/video/video.svg
    /// </summary>
    public const string VideoSvgPath = FileTypeFullBasePath + "video/video.svg";

    /// <summary>
    /// ms-appx:///Assets/115/file_type/other/unknown.svg
    /// </summary>
    public const string UnknownSvgPath = FileTypeFullBasePath + "other/unknown.svg";

    public static readonly Dictionary<string, Dictionary<string, List<string>>> FileTypeDictionary = new()
        {
            { "video", new Dictionary<string, List<string>>{
                {"3gp", ["3g2", "3gp", "3gp2", "3gpp"] },
                {"flv", ["flv"] },
                {"mkv", ["mkv"] },
                {"mov", ["mov"] },
                {"mp4", ["mp4", "mpeg4"] },
                {"rm", ["rm"] },
                {"rmvb", ["rmvb"] },
                {"swf", ["swf"] },
                {"video", ["mpe", "mpeg", "mpg", "asf", "ram", "m4v", "vob", "divx", "webm"] },
                {"wmv", ["wmv"] },
                {"mts", ["mts"] },
                {"mpg", ["mpg"] },
                {"dat", ["dat"] },
            } },
            { "application", new Dictionary<string, List<string>>{
                { "apk", ["apk"] },
                { "bat", ["bat"] },
                { "exe", ["exe"] },
                { "ipa", ["ipa"] },
                { "msi", ["msi"] },
            } },
            { "archive", new Dictionary<string, List<string>>{
                {"7z", ["7z"] },
                {"cab", ["cab"] },
                {"dmg", ["dmg"] },
                {"iso", ["iso"] },
                {"rar", ["rar", "tar"] },
                {"zip", ["zip"] },
            } },
            { "audio", new Dictionary<string, List<string>>
            {
                {"ape", ["ape"] },
                {"audio", ["wav", "midi", "mid", "flac", "aac", "m4a", "ogg", "amr"] },
                {"mp3", ["mp3"] },
                {"wma", ["wma"] },
            } },
            { "code", new Dictionary<string, List<string>>{
                {"code", ["c", "cpp", "asp", "js", "php", "tpl", "xml", "h", "cs", "plist", "py", "rb"] },
                {"css", ["css", "sass", "scss", "less"] },
                {"html", ["htm", "html"] },
            } },
            { "document", new Dictionary<string, List<string>>{
                {"ass", ["ass"] },
                {"chm", ["chm"] },
                {"doc", ["doc", "docx", "docm", "dot", "dotx", "dotm"] },
                {"key", ["key"] },
                {"log", ["log"] },
                {"numbers", ["numbers"] },
                {"pages", ["pages"] },
                {"pdf", ["pdf"] },
                {"ppt", ["ppt", "pptx", "pptm", "pps", "pot"] },
                {"srt", ["srt"] },
                {"ssa", ["ssa"] },
                {"torrent", ["torrent"] },
                {"txt", ["txt"] },
                {"xls", ["xls", "xlsx", "xlsm", "xltx", "xltm", "xlam", "xlsb"] },
            } },
            { "image", new Dictionary<string, List<string>>{
                {"gif", ["gif"] },
                {"img", ["bmp", "tiff", "exif"] },
                {"jpg", ["jpg"] },
                {"png", ["png"] },
                {"raw", ["raw"] },
                {"svg", ["svg"] }
            } },
            { "source", new Dictionary<string, List<string>>{
                {"ai", ["ai"] },
                {"fla", ["fla"] },
                {"psd", ["psd"] },
            } },
            { "folder", new Dictionary<string, List<string>>{
                {"folder", ["folder"] }
            } },
        };
}
