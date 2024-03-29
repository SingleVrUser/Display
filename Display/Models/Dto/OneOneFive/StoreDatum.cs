namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 存储Cid下面的文件列表
/// </summary>
public class StoreDatum
{
    public long Cid { get; set; }
    public Datum[] DatumList { get; set; }
}