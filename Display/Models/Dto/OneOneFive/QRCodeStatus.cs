namespace Display.Models.Dto.OneOneFive;

public class QRCodeStatus
{
    public int state { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public StatusInfo data { get; set; }
}