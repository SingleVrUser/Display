using DataAccess.Models.Entity;

namespace Display.Models.Vo;

public class FailVideoInfo : VideoInfoVo
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

    public FailVideoInfo(FilesInfo failDatum)
    {
        TrueName = failDatum.Name;
        ImagePath = "ms-appx:///Assets/Fail.jpg";
        Url = ImagePath;
        Series = "fail";
        ReleaseTime = failDatum.Time;

        Cid = failDatum.CurrentId;
        Fid = failDatum.FileId;
        PickCode = failDatum.PickCode;
        FileName = failDatum.Name;
        Size = failDatum.Size;
    }
}