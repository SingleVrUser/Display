using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

public class FailListIsLikeLookLater : BaseEntity
{
    [StringLength(20)]
    public required string PickCode { get; set; }

    public int? IsLike { get; set; }

    public int? Score { get; set; }

    public long LookLater { get; set; }

    [StringLength(500)]
    public string? ImagePath { get; set; }
}
