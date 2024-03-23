using System.Collections.Generic;

namespace Display.Constants;

public static class FileType
{
    public const string MsUri = "ms-appx:///";

    public const string AssetsBasePath = "Assets/";

    /// <summary>
    /// ms-appx:///Assets/
    /// </summary>
    public const string AssetsFullBasePath = MsUri + AssetsBasePath;

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
                {"3gp",new List<string>{ "3g2", "3gp", "3gp2", "3gpp" } },
                {"flv",new List<string>{"flv"} },
                {"mkv",new List<string>{"mkv"} },
                {"mov",new List<string>{"mov"} },
                {"mp4",new List<string>{ "mp4", "mpeg4" } },
                {"rm",new List<string>{"rm"} },
                {"rmvb",new List<string>{"rmvb"} },
                {"swf",new List<string>{"swf"} },
                {"video",new List<string>{ "mpe", "mpeg", "mpg", "asf", "ram", "m4v", "vob", "divx", "webm" } },
                {"wmv",new List<string>{"wmv"} },
                {"mts",new List<string>{"mts"} },
                {"mpg",new List<string>{"mpg"} },
                {"dat",new List<string>{"dat"} },
            } },
            { "application", new Dictionary<string, List<string>>{
                { "apk",new List<string> {"apk"}  },
                { "bat",new List<string> { "bat" }  },
                { "exe",new List<string> { "exe" }  },
                { "ipa",new List<string> { "ipa" }  },
                { "msi",new List<string> {"msi"}  },
            } },
            { "archive", new Dictionary<string, List<string>>{
                {"7z",new List<string> {"7z"} },
                {"cab",new List<string> {"cab"} },
                {"dmg",new List<string> {"dmg"} },
                {"iso",new List<string> {"iso"} },
                {"rar",new List<string> {"rar", "tar"} },
                {"zip",new List<string> {"zip"} },
            } },
            { "audio", new Dictionary<string, List<string>>
            {
                {"ape",new List<string> {"ape"} },
                {"audio",new List<string> {"wav","midi","mid","flac","aac","m4a","ogg","amr"}},
                {"mp3",new List<string> {"mp3"} },
                {"wma",new List<string> {"wma"} },
            } },
            { "code", new Dictionary<string, List<string>>{
                {"code",new List<string> { "c", "cpp", "asp", "js", "php", "tpl", "xml", "h", "cs", "plist", "py", "rb" } },
                {"css",new List<string> { "css", "sass", "scss", "less" } },
                {"html",new List<string> { "htm", "html" } },
            } },
            { "document", new Dictionary<string, List<string>>{
                {"ass",new List<string> {"ass"} },
                {"chm",new List<string> {"chm"} },
                {"doc",new List<string> { "doc", "docx", "docm", "dot", "dotx", "dotm" } },
                {"key",new List<string> {"key"} },
                {"log",new List<string> {"log"} },
                {"numbers",new List<string> {"numbers"} },
                {"pages",new List<string> {"pages"} },
                {"pdf",new List<string>{"pdf"} },
                {"ppt",new List<string>{ "ppt", "pptx", "pptm", "pps", "pot" } },
                {"srt",new List<string>{"srt"} },
                {"ssa",new List<string>{"ssa"} },
                {"torrent",new List<string>{"torrent"} },
                {"txt",new List<string>{"txt"} },
                {"xls",new List<string>{ "xls", "xlsx", "xlsm", "xltx", "xltm", "xlam", "xlsb" } },
            } },
            { "image", new Dictionary<string, List<string>>{
                {"gif",new List<string>{"gif"} },
                {"img",new List<string>{ "bmp", "tiff", "exif" } },
                {"jpg",new List<string>{"jpg"} },
                {"png",new List<string>{"png"} },
                {"raw",new List<string>{"raw"} },
                {"svg",new List<string>{ "svg" } }
            } },
            { "source", new Dictionary<string, List<string>>{
                {"ai",new List<string>{ "ai" } },
                {"fla",new List<string>{"fla"} },
                {"psd",new List<string>{"psd"} },
            } },
            { "folder", new Dictionary<string, List<string>>{
                {"folder",new List<string>{ "folder" } }
            } },
        };
}
