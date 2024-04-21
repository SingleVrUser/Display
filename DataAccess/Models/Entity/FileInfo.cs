using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DataAccess.Models.Entity;

public class FileInfo : BaseEntity
{
    [JsonProperty("fid")]
    public long FileId { get; set; }

    [JsonProperty("uid")]
    public long Uid { get; set; }

    [JsonProperty("aid")]
    public int? Aid { get; set; }

    /// <summary>
    /// 当前目录Id
    /// </summary>
    [JsonProperty("cid")]
    public long CurrentId { get; set; }

    /// <summary>
    /// 父级目录Id
    /// </summary>
    [JsonProperty("pid")]
    public long? ParentId { get; set; }
    
    [JsonProperty("n")]
    [StringLength(200)]
    public required string Name { get; init; }

    [JsonProperty("s")]
    public long Size { get; set; }

    [JsonProperty("sta")]
    public int? Sta { get; set; }

    [JsonProperty("pt")]
    public string? Pt { get; set; }

    [JsonProperty("pc")]
    [StringLength(18)]
    public required string PickCode { get; init; }

    [JsonProperty("p")]
    public int? P { get; set; }

    [JsonProperty("m")]
    public int? M { get; set; }

    [JsonProperty("t")]
    public required string Time { get; set; }

    [JsonProperty("te")]
    public int TimeEdit { get; set; }

    [JsonProperty("tp")]
    public int TimeProduce { get; set; }

    [JsonProperty("d")]
    public int? D { get; set; }

    [JsonProperty("c")]
    public int? C { get; set; }

    [JsonProperty("sh")]
    public int? Sh { get; set; }

    [JsonProperty("e")]
    public string? E { get; set; }

    [JsonProperty("ico")]
    public string? Ico { get; set; }

    [JsonProperty("sha")]
    public string? Sha { get; set; }

    [JsonProperty("fdes")]
    public string? Fdes { get; set; }

    [JsonProperty("q")]
    public int? Q { get; set; }

    [JsonProperty("hdf")]
    public int? Hdf { get; set; }

    [JsonProperty("fvs")]
    public int? Fvs { get; set; }

    [JsonProperty("u")]
    public string? U { get; set; }

    [JsonProperty("iv")]
    public int? Iv { get; set; }

    [JsonProperty("current_time")]
    public int? CurrentTime { get; set; }

    [JsonProperty("played_end")]
    public int? PlayedEnd { get; set; }

    [JsonProperty("last_time")]
    public string? LastTime { get; set; }

    [JsonProperty("vdi")]
    public int Vdi { get; set; }

    [JsonProperty("play_long")]
    public double? PlayLong { get; set; }

    #region 外键
    
    [ForeignKey(nameof(VideoInfo))]
    public long? VideoId { get; set; }
    
    #endregion
    
    public VideoInfo? VideoInfo { get; set; }
    
    
}
