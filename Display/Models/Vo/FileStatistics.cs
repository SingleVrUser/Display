using System.Collections.Generic;
using Display.Models.Enums;

namespace Display.Views.Pages.SpiderVideoInfo;

public class FileStatistics(FileFormatEnum name)
{
    public FileFormatEnum Type { get; set; } = name;
    public long Size { get; set; }
    public int Count { get; set; }
    public List<Data> FileInfo { get; set; } = [];

    public class Data
    {
        public string Name { get; init; }
        public int Count { get; set; }
        public long Size { get; set; }
    }
}