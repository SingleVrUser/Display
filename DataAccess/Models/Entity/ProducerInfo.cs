using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;


public class ProducerInfo : BaseEntity
{
    [StringLength(10)]
    public required string Name { get; set; }

    public bool IsWm { get; set; }
}