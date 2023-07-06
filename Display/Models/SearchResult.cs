using Display.Data;

namespace Display.Models
{
    public class SearchResult
    {
        public int count { get; set; }
        public SearchDatum[] data { get; set; }
        public float cost_time_es { get; set; }
        public int file_count { get; set; }
        public int folder_count { get; set; }
        public Folder folder { get; set; }
        public int page_size { get; set; }
        public int offset { get; set; }
        public int is_asc { get; set; }
        public string order { get; set; }
        public string suffix { get; set; }
        public int fc_mix { get; set; }
        public int type { get; set; }
        public bool state { get; set; }
        public string error { get; set; }
        public int errCode { get; set; }
    }


    public class Folder
    {
        public string name { get; set; }
        public string cid { get; set; }
        public string pid { get; set; }
    }

    public class SearchDatum
    {
        public string n { get; set; }
        public long cid { get; set; }
        public long? fid { get; set; }
        public long? pid { get; set; }
        public string aid { get; set; }
        public string m { get; set; }
        public string cc { get; set; }
        public string sh { get; set; }
        public string pc { get; set; }
        public string t { get; set; }
        public int te { get; set; }
        public int tp { get; set; }
        public int d { get; set; }
        public string e { get; set; }
        public string dp { get; set; }
        public int p { get; set; }
        public string ns { get; set; }
        public int hdf { get; set; }
        public int ispl { get; set; }
        public int check_code { get; set; }
        public string check_msg { get; set; }
        public Fl[] fl { get; set; }
        public int issct { get; set; }
        public long s { get; set; }
        public string sta { get; set; }
        public string pt { get; set; }
        public string c { get; set; }
        public string ico { get; set; }
        public string sha { get; set; }
        public int q { get; set; }
        public string ih { get; set; }
        public int nm { get; set; }
        public string u { get; set; }
        public int iv { get; set; }
        public int vdi { get; set; }
        public int play_long { get; set; }
    }
}
