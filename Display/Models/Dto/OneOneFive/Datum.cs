using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class Datum
{
    [JsonProperty(propertyName: "fid")]
    public long? Fid { get; set; }

    [JsonProperty(propertyName: "uid")]
    public long Uid { get; set; }

    [JsonProperty(propertyName: "aid")]
    public int Aid { get; set; }


    [JsonProperty(propertyName: "cid")]
    public long Cid { get; set; }

    [JsonProperty(propertyName: "n")]
    public string Name { get; set; }

    [JsonProperty(propertyName: "s")]
    public long Size { get; set; }

    [JsonProperty(propertyName: "sta")]
    public int Sta { get; set; }


    [JsonProperty(propertyName: "pt")]
    public string Pt { get; set; }
    [JsonProperty(propertyName: "pid")]
    public long? Pid { get; set; }

    [JsonProperty(propertyName: "pc")]
    public string PickCode { get; set; }



    [JsonProperty(propertyName: "p")]
    public int P { get; set; }


    [JsonProperty(propertyName: "m")]
    public int M { get; set; }

    [JsonProperty(propertyName: "t")]
    public string Time { get; set; }


    [JsonProperty(propertyName: "te")]
    public int TimeEdit { get; set; }

    [JsonProperty(propertyName: "tp")]
    public int TimeProduce { get; set; }

    [JsonProperty(propertyName: "d")]
    public int D { get; set; }

    [JsonProperty(propertyName: "c")]
    public int C { get; set; }


    [JsonProperty(propertyName: "sh")]
    public int Sh { get; set; }

    [JsonProperty(propertyName: "e")]
    public string E { get; set; }


    [JsonProperty(propertyName: "ico")]
    public string Ico { get; set; }


    [JsonProperty(propertyName: "sha")]
    public string Sha { get; set; }


    [JsonProperty(propertyName: "fdes")]
    public string Fdes { get; set; }


    [JsonProperty(propertyName: "q")]
    public int Q { get; set; }


    [JsonProperty(propertyName: "hdf")]
    public int Hdf { get; set; }


    [JsonProperty(propertyName: "fvs")]
    public int Fvs { get; set; }


    public Fl[] Fl { get; set; }


    [JsonProperty(propertyName: "u")]
    public string U { get; set; }


    [JsonProperty(propertyName: "iv")]
    public int Iv { get; set; }


    [JsonProperty(propertyName: "current_time")]
    public int CurrentTime { get; set; }


    [JsonProperty(propertyName: "played_end")]
    public int PlayedEnd { get; set; }


    [JsonProperty(propertyName: "last_time")]
    public string LastTime { get; set; }


    [JsonProperty(propertyName: "vdi")]
    public int Vdi { get; set; }


    [JsonProperty(propertyName: "play_long")]
    public double PlayLong { get; set; }

    public override string ToString() => Name;
}