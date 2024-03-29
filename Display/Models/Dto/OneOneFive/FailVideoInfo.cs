namespace Display.Models.Dto.OneOneFive;

public class FailVideoInfo : VideoCoverDisplayClass
{
    public long Cid { get; set; }

    public long? Fid { get; set; }

    public string PickCode { get; set; }

    public string FileName { get; set; }

    public long Size { get; set; }


    public FailVideoInfo(VideoInfo info, double imgWidth, double imgHeight)
        : base(info, imgWidth, imgHeight)
    {

    }

    public FailVideoInfo(Datum failDatum)
    {
        trueName = failDatum.Name;
        ImagePath = "ms-appx:///Assets/Fail.jpg";
        busUrl = ImagePath;
        Series = "fail";
        ReleaseTime = failDatum.Time;

        Cid = failDatum.Cid;
        Fid = failDatum.Fid;
        PickCode = failDatum.PickCode;
        FileName = failDatum.Name;
        Size = failDatum.Size;
    }
}