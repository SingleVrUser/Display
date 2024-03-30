using Display.Models.Enums.OneOneFive;

namespace Display.Models.Vo.Progress;

public class GetFileProgressIProgress
{
    public ProgressStatus status { get; set; } = ProgressStatus.normal;
    public GetFilesProgressInfo getFilesProgressInfo { get; set; }
    public long sendCountPerMinutes { get; set; }
}