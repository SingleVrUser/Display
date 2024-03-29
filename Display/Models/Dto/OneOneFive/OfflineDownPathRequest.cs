namespace Display.Models.Dto.OneOneFive;

public class OfflineDownPathRequest
{
    public bool state { get; set; }
    public object error { get; set; }
    public object errno { get; set; }
    public OfflineDownPathData[] data { get; set; }

}