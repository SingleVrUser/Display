using ByteSizeLib;
using Display.Helper.Date;
using Display.Models.Api.OneOneFive.File;

namespace Display.Models.Vo;

public class FileCategory
{
    /// <summary>
    /// 文件数量（不包括文件夹）
    /// </summary>
    public int count { get; set; }
    
    public string size { get; set; }
    /// <summary>
    /// 文件夹数量（不包括文件）
    /// </summary>
    public int folder_count { get; set; }
    
    public int show_play_long { get; set; }

    private int ptime { get; set; }
    
    public int utime { get; set; }
    
    public int is_share { get; set; }
    
    public string file_name { get; set; }
    
    public string pick_code { get; set; }
    
    public string sha1 { get; set; }
    
    public int is_mark { get; set; }
    
    public int open_time { get; set; }
    
    public string desc { get; set; }
    
    public int file_category { get; set; }
    
    public ParentPath[] paths { get; set; }

    public int allCount => count + folder_count;

    public static Datum ConvertFolderToDatum(FileCategory fileCategory, long cid)
    {
        Datum datum = new()
        {
            Uid = 0,
            Aid = 1,
            Cid = cid,
            Name = fileCategory.file_name,
            Pid = fileCategory.paths[^1].FileId,
            PickCode = fileCategory.pick_code,
            Time = DateHelper.ConvertInt32ToDateTime(fileCategory.utime),
            TimeProduce = fileCategory.ptime,
            TimeEdit = fileCategory.utime,
        };

        return datum;
    }

    public FileCategory()
    {

    }

    public FileCategory(Datum datum)
    {
        file_name = datum.Name;
        pick_code = datum.PickCode;
        count = 1;
        file_category = 1;
        utime = datum.TimeEdit;
        ptime = datum.TimeProduce;
        sha1 = datum.Sha1;
        size = ByteSize.FromBytes(datum.Size).ToString("#.#");
    }
}