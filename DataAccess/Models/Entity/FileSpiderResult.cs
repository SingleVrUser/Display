using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Entity;

/// <summary>
/// 视频信息搜刮结果
/// </summary>
public class FileSpiderResult : BaseEntity
{
    public long FileId { get; set; }
    
    [StringLength(10)]
    public string? TrueName { get; set; }
    
    public bool IsSuccess { get; set; }
    
    // public FileInfo FileInfo { get; set; }
}