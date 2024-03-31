using Display.Models.Dto.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

public class Datum
{
    [JsonProperty("fid")]
    public long? Fid { get; set; }

    [JsonProperty("uid")]
    public long Uid { get; set; }

    [JsonProperty("aid")]
    public int Aid { get; set; }

    [JsonProperty("cid")]
    public long Cid { get; set; }

    [JsonProperty("n")]
    public string Name { get; set; }

    [JsonProperty("s")]
    public long Size { get; set; }

    [JsonProperty("sta")]
    public int Sta { get; set; }

    [JsonProperty("pt")]
    public string Pt { get; set; }
    
    [JsonProperty("pid")]
    public long? Pid { get; set; }

    [JsonProperty("pc")]
    public string PickCode { get; set; }

    [JsonProperty("p")]
    public int P { get; set; }


    [JsonProperty("m")]
    public int M { get; set; }

    [JsonProperty("t")]
    public string Time { get; set; }


    [JsonProperty("te")]
    public int TimeEdit { get; set; }

    [JsonProperty("tp")]
    public int TimeProduce { get; set; }

    [JsonProperty("d")]
    public int D { get; set; }

    [JsonProperty("c")]
    public int C { get; set; }


    [JsonProperty("sh")]
    public int Sh { get; set; }

    [JsonProperty("e")]
    public string E { get; set; }


    [JsonProperty("ico")]
    public string Ico { get; set; }


    [JsonProperty("sha")]
    public string Sha1 { get; set; }


    [JsonProperty("fdes")]
    public string Fdes { get; set; }


    [JsonProperty("q")]
    public int Q { get; set; }


    [JsonProperty("hdf")]
    public int Hdf { get; set; }


    [JsonProperty("fvs")]
    public int Fvs { get; set; }

    [JsonProperty("fl")]
    public Flag[] Fl { get; set; }

    [JsonProperty("u")]
    public string U { get; set; }


    [JsonProperty("iv")]
    public int Iv { get; set; }


    [JsonProperty("current_time")]
    public int CurrentTime { get; set; }


    [JsonProperty("played_end")]
    public int PlayedEnd { get; set; }


    [JsonProperty("last_time")]
    public string LastTime { get; set; }


    [JsonProperty("vdi")]
    public int Vdi { get; set; }


    [JsonProperty("play_long")]
    public double PlayLong { get; set; }

    public override string ToString() => Name;
}