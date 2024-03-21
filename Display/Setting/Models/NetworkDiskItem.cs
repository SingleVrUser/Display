using System.Collections.Generic;

namespace Display.Setting.Models;

public class NetworkDiskItem
{
    public string Name { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public bool IsRecordDownRequest { get; set; }
    public double DownUrlOverdueTime { get; set; }

    public SavePath DefaultSavePath { get; set; }

    public class SavePath
    {
        public string Name { get; set; }

        public long Id { get; set; }
    }
}