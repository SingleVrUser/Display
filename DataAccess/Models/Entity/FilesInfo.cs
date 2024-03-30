using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("FilesInfo")]
public class FilesInfo
{
    [Column("fid")]
    [StringLength(20)]
    public string? Fid { get; init; }

    [Column("uid")]
    public int? Uid { get; init; }

    [Column("aid")]
    public int? Aid { get; init; }

    [Column("cid")]
    [StringLength(20)]
    public string? Cid { get; init; }

    [Column("n")]
    public string? N { get; init; }

    [Column("s")]
    public int? S { get; init; }

    [Column("sta")]
    public int? Sta { get; init; }

    [Column("pt")]
    public string? Pt { get; init; }

    [Column("pid")]
    public string? Pid { get; init; }

    [Key]
    [Column("pc")]
    public string Pc { get; init; } = null!;

    [Column("p")]
    public int? P { get; init; }

    [Column("m")]
    public int? M { get; init; }

    [Column("t")]
    public string? T { get; init; }

    [Column("te")]
    public int? Te { get; init; }

    [Column("tp")]
    public int? Tp { get; init; }

    [Column("d")]
    public int? D { get; init; }

    [Column("c")]
    public int? C { get; init; }

    [Column("sh")]
    public int? Sh { get; init; }

    [Column("e")]
    public string? E { get; init; }

    [Column("ico")]
    public string? Ico { get; init; }

    [Column("sha")]
    public string? Sha { get; init; }

    [Column("fdes")]
    public string? Fdes { get; init; }

    [Column("q")]
    public int? Q { get; init; }

    [Column("hdf")]
    public int? Hdf { get; init; }

    [Column("fvs")]
    public int? Fvs { get; init; }

    [Column("u")]
    public string? U { get; init; }

    [Column("iv")]
    public int? Iv { get; init; }

    [Column("current_time")]
    public int? CurrentTime { get; init; }

    [Column("played_end")]
    public int? PlayedEnd { get; init; }

    [Column("last_time")]
    public string? LastTime { get; init; }

    [Column("vdi")]
    public int? Vdi { get; init; }

    [Column("play_long")]
    public double? PlayLong { get; init; }
}
