namespace Display.Models.Dto.OneOneFive;

public class WebFileInfo
{
    public Datum[] data { get; set; }
    public WebPath[] path { get; set; }

    public int count { get; set; }
    public string data_source { get; set; }
    public int sys_count { get; set; }
    public int offset { get; set; }
    public int o { get; set; }
    public int limit { get; set; }
    public int page_size { get; set; }
    public int aid { get; set; }
    public long cid { get; set; }
    public int is_asc { get; set; }
    public string order { get; set; }
    public int star { get; set; }
    public int type { get; set; }
    public int r_all { get; set; }
    public int stdir { get; set; }
    public int cur { get; set; }
    public int fc_mix { get; set; }
    public string suffix { get; set; }
    public int min_size { get; set; }
    public int max_size { get; set; }
    public bool state { get; set; }
    public string error { get; set; }
    public int errNo { get; set; }
}