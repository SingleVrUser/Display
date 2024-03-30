using System.Text.RegularExpressions;
using Display.Models.Enums;

namespace Display.Models.Vo.Forum;

public class Forum1080AttachmentInfo(string url, AttmnTypeEnum typeEnum)
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

    public string Url { get; set; } = url;

    public string Name { get; set; }

    public string Size { get; set; }

    public AttmnTypeEnum TypeEnum { get; set; } = typeEnum;

    public int Expense { get; set; } = 0;

    public int DownCount { get; set; }
}