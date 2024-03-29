namespace Display.Models.Dto.OneOneFive;

public class FailDatum
{
    public FailDatum(Datum Datum)
    {
        this.Datum = Datum;
    }

    public Datum Datum { get; private set; }

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