using System.Collections.Generic;

namespace Display.Models.Dto.OneOneFive;

public class RenameRequest
{
    public bool state { get; set; }
    public string error { get; set; }
    public int errno { get; set; }
    public Dictionary<string, string> data { get; set; }
}