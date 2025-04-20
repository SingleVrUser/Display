using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DataAccess.Models.Entity;

[Table("FilesInfo")]
public class FilesInfo
{
    [Column("fid")]
    [JsonProperty("fid")]
    [StringLength(20)]
    public long FileId { get; set; }

    [Column("uid")]
    [JsonProperty("uid")]
    public long Uid { get; set; } = 0;

    [Column("aid")]
    [JsonProperty("aid")]
    public int? Aid { get; set; }

    /// <summary>
    /// 当前目录Id
    /// </summary>
    [Column("cid")]
    [JsonProperty("cid")]
    public long CurrentId { get; set; }

    /// <summary>
    /// 父级目录Id
    /// </summary>
    [Column("pid")]
    [JsonProperty("pid")]
    public long? ParentId { get; set; }
    
    [Column("n")]
    [JsonProperty("n")]
    [StringLength(200)]
    public string Name { get; set; }

    [Column("s")]
    [JsonProperty("s")]
    public long Size { get; set; }

    [Column("sta")]
    [JsonProperty("sta")]
    public int? Sta { get; set; }

    [Column("pt")]
    [JsonProperty("pt")]
    public string? Pt { get; set; }

    [Key]
    [Column("pc")]
    [JsonProperty("pc")]
    [StringLength(18)]
    public string PickCode { get; set; } = null!;

    [Column("p")]
    [JsonProperty("p")]
    public int? P { get; set; }

    [Column("m")]
    [JsonProperty("m")]
    public int? M { get; set; }

    [Column("t")]
    [JsonProperty("t")]
    public string Time { get; set; }

    [Column("te")]
    [JsonProperty("te")]
    public int TimeEdit { get; set; }

    [Column("tp")]
    [JsonProperty("tp")]
    public int TimeProduce { get; set; }

    [Column("d")]
    [JsonProperty("d")]
    public int? D { get; set; }

    [Column("c")]
    [JsonProperty("c")]
    public int? C { get; set; }

    [Column("sh")]
    [JsonProperty("sh")]
    public int? Sh { get; set; }

    [Column("e")]
    [JsonProperty("e")]
    public string? E { get; set; }

    [Column("ico")]
    [JsonProperty("ico")]
    public string? Ico { get; set; }

    [Column("sha")]
    [JsonProperty("sha")]
    public string? Sha { get; set; }

    [Column("fdes")]
    [JsonProperty("fdes")]
    public string? Fdes { get; set; }

    [Column("q")]
    [JsonProperty("q")]
    public int? Q { get; set; }

    [Column("hdf")]
    [JsonProperty("hdf")]
    public int? Hdf { get; set; }

    [Column("fvs")]
    [JsonProperty("fvs")]
    public int? Fvs { get; set; }

    [Column("u")]
    [JsonProperty("u")]
    public string? U { get; set; }

    [Column("iv")]
    [JsonProperty("iv")]
    public int? Iv { get; set; }

    [Column("current_time")]
    [JsonProperty("current_time")]
    public int? CurrentTime { get; set; }

    [Column("played_end")]
    [JsonProperty("played_end")]
    public int? PlayedEnd { get; set; }

    [Column("last_time")]
    [JsonProperty("last_time")]
    public string? LastTime { get; set; }

    [Column("vdi")]
    [JsonProperty("vdi")]
    public int Vdi { get; set; }

    [Column("play_long")]
    [JsonProperty("play_long")]
    public double? PlayLong { get; set; }
}
