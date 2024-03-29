namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 浏览器选择项
/// </summary>
public class SelectedItem
{
    public long id { get; set; }
    public string name { get; set; }
    public int file_count { get; set; }
    public int folder_count { get; set; }
    public string size { get; set; }
    public string pick_code { get; set; }
    public int file_type { get; set; }
    public long file_id { get; set; }

    /// <summary>
    /// 是否有隐藏文件，有为1，无为0
    /// </summary>
    public int hasHiddenFile { get; set; }
}