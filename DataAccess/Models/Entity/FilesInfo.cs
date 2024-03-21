using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("FilesInfo")]
public class FilesInfo
{
    [Column("fid")]
    public string? Fid { get; set; }

    [Column("uid")]
    public int? Uid { get; set; }

    [Column("aid")]
    public int? Aid { get; set; }

    [Column("cid")]
    public string? Cid { get; set; }

    [Column("n")]
    public string? N { get; set; }

    [Column("s")]
    public int? S { get; set; }

    [Column("sta")]
    public int? Sta { get; set; }

    [Column("pt")]
    public string? Pt { get; set; }

    [Column("pid")]
    public string? Pid { get; set; }

    [Key]
    [Column("pc")]
    public string Pc { get; set; } = null!;

    [Column("p")]
    public int? P { get; set; }

    [Column("m")]
    public int? M { get; set; }

    [Column("t")]
    public string? T { get; set; }

    [Column("te")]
    public int? Te { get; set; }

    [Column("tp")]
    public int? Tp { get; set; }

    [Column("d")]
    public int? D { get; set; }

    [Column("c")]
    public int? C { get; set; }

    [Column("sh")]
    public int? Sh { get; set; }

    [Column("e")]
    public string? E { get; set; }

    [Column("ico")]
    public string? Ico { get; set; }

    [Column("sha")]
    public string? Sha { get; set; }

    [Column("fdes")]
    public string? Fdes { get; set; }

    [Column("q")]
    public int? Q { get; set; }

    [Column("hdf")]
    public int? Hdf { get; set; }

    [Column("fvs")]
    public int? Fvs { get; set; }

    [Column("u")]
    public string? U { get; set; }

    [Column("iv")]
    public int? Iv { get; set; }

    [Column("current_time")]
    public int? CurrentTime { get; set; }

    [Column("played_end")]
    public int? PlayedEnd { get; set; }

    [Column("last_time")]
    public string? LastTime { get; set; }

    [Column("vdi")]
    public int? Vdi { get; set; }

    [Column("play_long")]
    public double? PlayLong { get; set; }
}
