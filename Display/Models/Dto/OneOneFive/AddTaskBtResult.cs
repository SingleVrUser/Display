namespace Display.Models.Dto.OneOneFive;

public class AddTaskBtResult
{
    public bool state { get; set; }
    public int errno { get; set; }
    public string errtype { get; set; }
    public int errcode { get; set; }
    public string info_hash { get; set; }
    public string name { get; set; }
    public int start_torrent { get; set; }
}