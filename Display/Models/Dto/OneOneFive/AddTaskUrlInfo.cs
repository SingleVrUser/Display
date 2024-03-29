namespace Display.Models.Dto.OneOneFive;

public class AddTaskUrlInfo
{
    public bool state { get; set; }
    public int errno { get; set; }

    public int errcode { get; set; }

    public string errtype { get; set; }

    public string error_msg { get; set; }
    public string info_hash { get; set; }

    public string url { get; set; }

    public AddTaskUrlInfo[] result { get; set; }
}