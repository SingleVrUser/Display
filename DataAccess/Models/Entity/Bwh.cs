using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;


public class Bwh : BaseEntity
{
    [StringLength(100)]
    public string BwhContent { get; init; } = null!;

    public int Bust { get; init; }

    public int Waist { get; init; }

    public int Hips { get; init; }

    public Bwh(){}

    public Bwh(string bwhContent)
    {
        BwhContent = bwhContent;

        var splitArray = bwhContent.Split('_');

        if (splitArray.Length != 3) return;
        if(int.TryParse(splitArray[0], out var bust)) Bust = bust ;
        if(int.TryParse(splitArray[1], out var waist)) Waist = waist;
        if(int.TryParse(splitArray[2], out var hips)) Hips = hips;
    }
}
