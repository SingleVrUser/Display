using System.Collections.Generic;

namespace Display.Models.Dto;

public class MatchVideoResult
{
    public string MatchName { get; set; }
    public List<long> FileIdList { get; set; }
}