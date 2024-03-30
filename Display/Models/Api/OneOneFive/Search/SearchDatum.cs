using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Search;

public class SearchDatum
{
    [JsonProperty(propertyName: "n")]
    public string Name { get; set; }

    [JsonProperty(propertyName: "cid")]
    public long Cid { get; set; }

    [JsonProperty(propertyName: "fid")]
    public long? Fid { get; set; }

    [JsonProperty(propertyName: "pid")]
    public long? Pid { get; set; }

    [JsonProperty(propertyName: "aid")]
    public string Aid { get; set; }

    [JsonProperty(propertyName: "m")]
    public string M { get; set; }

    [JsonProperty(propertyName: "cc")]
    public string Cc { get; set; }

    [JsonProperty(propertyName: "sh")]
    public string Sh { get; set; }

    [JsonProperty(propertyName: "pc")]
    public string Pc { get; set; }

    [JsonProperty(propertyName: "t")]
    public string T { get; set; }

    [JsonProperty(propertyName: "te")]
    public int TimeEdit { get; set; }

    [JsonProperty(propertyName: "tp")]
    public int Tp { get; set; }

    [JsonProperty(propertyName: "d")]
    public int D { get; set; }

    [JsonProperty(propertyName: "e")]
    public string E { get; set; }

    [JsonProperty(propertyName: "dp")]
    public string Dp { get; set; }

    [JsonProperty(propertyName: "p")]
    public int P { get; set; }

    [JsonProperty(propertyName: "ns")]
    public string Ns { get; set; }

    [JsonProperty(propertyName: "hdf")]
    public int Hdf { get; set; }

    [JsonProperty(propertyName: "ispl")]
    public int IsPl { get; set; }

    [JsonProperty(propertyName: "check_code")]
    public int CheckCode { get; set; }

    [JsonProperty(propertyName: "check_msg")]
    public string CheckMsg { get; set; }

    [JsonProperty(propertyName: "fl")]
    public Flag[] Fl { get; set; }

    [JsonProperty(propertyName: "issct")]
    public int IsSct { get; set; }

    [JsonProperty(propertyName: "s")]
    public long Size { get; set; }

    [JsonProperty(propertyName: "sta")]
    public string Sta { get; set; }

    [JsonProperty(propertyName: "pt")]
    public string Pt { get; set; }

    [JsonProperty(propertyName: "c")]
    public string C { get; set; }

    [JsonProperty(propertyName: "ico")]
    public string Ico { get; set; }

    [JsonProperty(propertyName: "sha")]
    public string Sha { get; set; }

    [JsonProperty(propertyName: "q")]
    public int Q { get; set; }

    [JsonProperty(propertyName: "ih")]
    public string Ih { get; set; }

    [JsonProperty(propertyName: "nm")]
    public int Nm { get; set; }

    [JsonProperty(propertyName: "u")]
    public string U { get; set; }

    [JsonProperty(propertyName: "iv")]
    public int Iv { get; set; }

    [JsonProperty(propertyName: "vdi")]
    public int Vdi { get; set; }

    [JsonProperty(propertyName: "play_long")]
    public double PlayLong { get; set; }
}