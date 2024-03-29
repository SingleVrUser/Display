namespace Display.Models.Upload;

public class OssUploadResultData
{
    public string pick_code { get; set; }
    public long file_size { get; set; }
    public long file_id { get; set; }
    public string thumb_url { get; set; }
    public string sha1 { get; set; }
    public int aid { get; set; }
    public string file_name { get; set; }
    public long cid { get; set; }
    public int is_video { get; set; }
}