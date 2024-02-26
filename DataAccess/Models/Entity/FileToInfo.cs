using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;

[Table("FileToInfo")]
public class FileToInfo
{
    [Key]
    [Column("file_pickcode")]
    [MinLength(5)]
    [MaxLength(20)]
    public string FilePickCode { get; set; } = null!;

    [Column("truename")]
    public string? Truename { get; set; }

    [Column("issuccess")]
    public int? Issuccess { get; set; }
}
