using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Entity;


public class VideoInterest : BaseEntity
{
    public long VideoId { get; set; }
    
    public bool IsLike { get; set; }

    public bool IsLookAfter { get; set; }

    public double? Score { get; set; }
}