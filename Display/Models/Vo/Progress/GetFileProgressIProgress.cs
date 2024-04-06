using Display.Models.Enums.OneOneFive;

namespace Display.Models.Vo.Progress;

public class GetFileProgressIProgress
{
    public ProgressStatus Status { get; set; } = ProgressStatus.normal;
    public GetFilesProgressInfo GetFilesProgressInfo { get; set; }
    public long SendCountPerMinutes { get; set; }
}