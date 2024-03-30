using System.Collections.Generic;

namespace Display.Models.Dto.OneOneFive;

public class VideoToThumbnail
{
    public string name;

    //总时长
    public float play_long { get; set; } = 0;

    //总帧数
    public double frame_count { get; set; } = 0;

    //pickCode
    public List<string> pickCodeList { get; set; } = new();

}