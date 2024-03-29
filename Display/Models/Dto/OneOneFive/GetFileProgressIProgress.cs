using Display.Models.Enums.OneOneFive;

namespace Display.Models.Dto.OneOneFive;

public class GetFileProgressIProgress
{
    public ProgressStatus status { get; set; } = ProgressStatus.normal;
    public GetFilesProgressInfo getFilesProgressInfo { get; set; }
    public long sendCountPerMinutes { get; set; }
}