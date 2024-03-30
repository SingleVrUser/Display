using System.Collections.Generic;

namespace Display.Models.Spider;

public class Forum1080AttmnFileInfo
{
    public Forum1080AttmnFileInfo(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    public string Name { get; private set; }

    private string Path { get; set; }
    public string QrCodeImagePath { get; set; }
    public string DetailFileContent { get; set; }
    public BaiduShareInfo BaiduShareInfo { get; set; }

    public string SrtPath { get; set; }

    public Dictionary<string, List<string>> Links { get; set; } = new();

    public string CompressedPassword { get; set; }
}