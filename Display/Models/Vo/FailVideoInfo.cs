using Display.Models.Api.OneOneFive.File;
using Display.Models.Entities.OneOneFive;
using Display.Models.Vo;

namespace Display.Models.Dto.OneOneFive;

public class FailVideoInfo : VideoCoverDisplayClass
{
    public long Cid { get; }

    public long? Fid { get; }

    public string PickCode { get; }

    public string FileName { get; }

    public long Size { get; }


    public FailVideoInfo(VideoInfo info, double imgWidth, double imgHeight)
        : base(info, imgWidth, imgHeight)
    {

    }

    public FailVideoInfo(Datum failDatum)
    {
        TrueName = failDatum.Name;
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