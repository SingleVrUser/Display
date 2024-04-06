using System.Collections.Generic;
using DataAccess.Models.Entity;
using Display.Models.Api.OneOneFive.File;

namespace Display.Models.Vo;

public class GetFilesProgressInfo
{
    public int FolderCount { get; set; } = 0;

    public int FilesCount { get; set; } = 0;

    public int AllCount => FolderCount + FilesCount;

    public List<long?> FailCid { get; set; } = [];
}