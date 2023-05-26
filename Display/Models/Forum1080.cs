using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WinRT.Interop;

namespace Display.Models
{
    public class Forum1080SearchResult
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Time { get; set; }

        public string Type { get; set; }

    }

    public class Forum1080AttachmentInfo
    {
        private string _aid;

        public string Aid
        {
            get
            {
                if (_aid != null) return _aid;

                var matchAid = Regex.Match(Url.Replace("%3D", ""), "aid=(\\w+)");
                if (matchAid.Success)
                {
                    _aid = matchAid.Groups[1].Value;
                }

                return _aid;
            }
        }

        public string Url { get; set; }

        public string Name { get; set; }

        public string Size { get; set; }

        public AttmnType Type { get; set; }

        public int Expense { get; set; } = 0;

        public int DownCount { get; set; }

        public Forum1080AttachmentInfo(string url, AttmnType type)
        {
            Url = url;
            Type = type;
        }

    }
    
    public class AttmnFileInfo
    {
        public AttmnFileInfo(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        public string Name { get; private set; }

        public string Path { get; set; }
        public string QrCodeImagePath { get; set; }
        public string DetailFileContent { get; set; }
        public BaiduShareInfo BaiduShareInfo { get; set; }

        public string SrtPath { get; set; }

        public Dictionary<string, List<string>> Links { get; set; } = new();

        public string CompressedPassword { get; set; }
    }

    public class BaiduShareInfo
    {
        public string ShareLink { get; set; }
        public string SharePassword { get; set; }
        public string ShareLinkWithPwd => $"{ShareLink}?pwd={SharePassword}";
    }

    public enum AttmnType
    {
        Magnet, Torrent, Rar, Txt, Unknown
    }


}
