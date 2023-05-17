using System;
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

        public int Expense { get; set; }

        public int DownCount { get; set; }

        public Forum1080AttachmentInfo(string url, AttmnType type)
        {
            Url = url;
            Type = type;
        }

    }
    public enum AttmnType
    {
        Magnet, Torrent, Rar, Txt, Unknown
    }


}
