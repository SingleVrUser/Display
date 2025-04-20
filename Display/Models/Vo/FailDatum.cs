using DataAccess.Models.Entity;
using Display.Models.Api.OneOneFive.File;

namespace Display.Models.Vo;

public class FailDatum(FilesInfo datum)
{
    public FilesInfo Datum { get; private set; } = datum;

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