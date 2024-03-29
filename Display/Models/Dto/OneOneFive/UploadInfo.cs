using System.Collections.Generic;

namespace Display.Models.Dto.OneOneFive;

public class UploadInfo
{
    public string uploadinfo { get; set; }
    public int user_id { get; set; }
    public int app_version { get; set; }
    public int app_id { get; set; }
    public string userkey { get; set; }
    public string url_upload { get; set; }
    public string url_resume { get; set; }
    public string url_cancel { get; set; }
    public string url_speed { get; set; }
    public Dictionary<string, string> url_speed_test { get; set; }
    public long size_limit { get; set; }
    public int size_limit_yun { get; set; }
    public int max_dir_level { get; set; }
    public int max_dir_level_yun { get; set; }
    public int max_file_num { get; set; }
    public int max_file_num_yun { get; set; }
    public bool upload_allowed { get; set; }
    public string upload_allowed_msg { get; set; }
    public List<string> type_limit { get; set; }
    public Dictionary<string, string> file_range { get; set; }
    public int isp_type { get; set; }
    public bool state { get; set; }
    public string error { get; set; }
    public int errno { get; set; }
}