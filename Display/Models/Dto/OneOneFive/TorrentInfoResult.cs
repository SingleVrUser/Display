namespace Display.Models.Dto.OneOneFive;

public class TorrentInfoResult
{
    public bool state { get; set; }
    public string error_msg { get; set; }
    public int errno { get; set; }
    public string errtype { get; set; }
    public int errcode { get; set; }
    public long file_size { get; set; }
    public string torrent_name { get; set; }
    public int file_count { get; set; }
    public string info_hash { get; set; }
    public Torrent_Filelist_Web[] torrent_filelist_web { get; set; }


}