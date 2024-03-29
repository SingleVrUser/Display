using System.Collections.Generic;

namespace Display.Models.Dto.OneOneFive;

public class ParamRequest
{
    public List<Down_Request> list { get; set; }
    public int count { get; set; }
    public string ref_url { get; set; }
}