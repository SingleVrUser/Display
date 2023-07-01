namespace Tests.Models
{
    public class FileInfo
    {
        public class Tv
        {
            public class Info
            {
                public Datum[] data { get; set; }
                public Path[] path { get; set; }
                public int count { get; set; }
                // 缺data_source
                public int sys_count { get; set; }
                public int offset { get; set; }
                // 缺o
                // 缺limit
                public int page_size { get; set; }
                public string aid { get; set; }
                public long cid { get; set; }
                public int is_asc { get; set; }
                public string order { get; set; }
                // 多
                public string sys_dir { get; set; }
                public int star { get; set; }

                public int type { get; set; }

                // 缺r_all
                public int stdir { get; set; }
                // 缺cur
                public int fc_mix { get; set; }
                // 缺suffix
                // 缺min_size
                // 缺max_size
                public bool state { get; set; }
                public string error { get; set; }
                public int errNo { get; set; }
            }

            public class Datum
            {
                public string category_id { get; set; }
                public string area_id { get; set; }
                public string parent_id { get; set; }
                public string user_id { get; set; }
                public string file_category { get; set; }
                public string category_name { get; set; }
                public string category_desc { get; set; }
                public string category_cover { get; set; }
                public int category_file_count { get; set; }
                public int category_order { get; set; }
                public string is_share { get; set; }
                public string is_mark { get; set; }
                public int is_private { get; set; }
                public string pick_code { get; set; }
                public string password { get; set; }
                public string ptime { get; set; }
                public string utime { get; set; }
                public string pptime { get; set; }
                public string ext1 { get; set; }
                public string ext2 { get; set; }
                public string ext3 { get; set; }
                public int has_pass { get; set; }
                public int type { get; set; }
                public int can_delete { get; set; }
                public int cate_mark { get; set; }
                public int show_play_long { get; set; }
                public object[] fl { get; set; }
            }

            public class Path
            {
                public string name { get; set; }
                public int aid { get; set; }
                public long cid { get; set; }
                public long pid { get; set; }

                public int isp { get; set; }

            }

        }

        public class Desktop
        {

            public class Info
            {
                public Datum[] data { get; set; }
                public int count { get; set; }
                public int sys_count { get; set; }
                public int offset { get; set; }
                public int limit { get; set; }
                public string aid { get; set; }
                public long cid { get; set; }
                public int is_asc { get; set; }
                public int min_size { get; set; }
                public int max_size { get; set; }
                public string sys_dir { get; set; }
                public string hide_data { get; set; }
                public string record_open_time { get; set; }
                public int star { get; set; }
                public int type { get; set; }
                public string suffix { get; set; }
                public Path[] path { get; set; }
                public int cur { get; set; }
                public int stdir { get; set; }
                public string fields { get; set; }
                public string order { get; set; }
                public int fc_mix { get; set; }
                public bool state { get; set; }
                public string error { get; set; }
                public int errno { get; set; }
            }

            public class Datum
            {
                public string fid { get; set; }
                public string aid { get; set; }
                public string pid { get; set; }
                public string fc { get; set; }
                public string fn { get; set; }
                public string fco { get; set; }
                public string ism { get; set; }
                public int isp { get; set; }
                public int iss { get; set; }
                public string pc { get; set; }
                public string upt { get; set; }
                public string uet { get; set; }
                public string uppt { get; set; }
                public int cm { get; set; }
                public string fdesc { get; set; }
                public int ispl { get; set; }
                public int fvs { get; set; }
                public int fuuid { get; set; }
                public int opt { get; set; }
                public object[] fl { get; set; }
                public int issct { get; set; }
            }

            public class Path
            {
                public string name { get; set; }
                public int aid { get; set; }
                public long cid { get; set; }
                public long pid { get; set; }
                public int isp { get; set; }
                public string p_cid { get; set; }
            }

        }



    }

}
