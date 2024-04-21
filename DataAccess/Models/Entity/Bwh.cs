using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;


public class Bwh : BaseEntity
{
    public int Bust { get; init; }

    public int Waist { get; init; }

    public int Hips { get; init; }
}
