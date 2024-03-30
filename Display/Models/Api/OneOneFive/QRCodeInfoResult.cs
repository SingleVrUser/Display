using Display.Models.Api.OneOneFive.User;

namespace Display.Models.Api.OneOneFive;

public class QRCodeInfoResult
{
    public int state { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public InfoData data { get; set; }
}