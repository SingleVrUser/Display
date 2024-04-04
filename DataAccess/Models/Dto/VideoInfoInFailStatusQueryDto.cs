using DataAccess.Models.Enum;

namespace DataAccess.Models.Dto;

public class VideoInfoInFailStatusQueryDto
{
    //int offset = 0,
    //int limit = -1,
    //string n = null,
    //string orderBy = null,
    //bool isDesc = false,

    public int Position { get; set; }

    public int Take { get; set; }

    public string? Name { get; set; }

    public bool IsRandom { get; set; }

    public SpiderFailType ShowType { get; set; }


}