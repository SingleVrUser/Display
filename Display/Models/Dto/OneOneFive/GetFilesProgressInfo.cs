using System.Collections.Generic;

namespace Display.Models.Dto.OneOneFive;

public class GetFilesProgressInfo
{
    public int FolderCount { get; set; } = 0;

    public int FilesCount { get; set; } = 0;

    public int AllCount => FolderCount + FilesCount;

    public List<long?> FailCid { get; set; } = [];

    public List<Datum> AddToDataAccessList = [];
}