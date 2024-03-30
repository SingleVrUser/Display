using Display.Models.Api.OneOneFive.File;

namespace Display.Models.Vo;

public class FailDatum(Datum datum)
{
    public Datum Datum { get; private set; } = datum;

    public string MatchName { get; set; }

    /// <summary>
    /// 是否是匹配失败
    /// </summary>
    public bool IsMatchFail { get; set; }

    /// <summary>
    /// 是否是搜刮失败
    /// </summary>
    public bool IsSpiderFail { get; set; }

}