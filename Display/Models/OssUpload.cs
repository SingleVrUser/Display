using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Models
{
    internal class MultipartUploadResult
    {
        public bool state { get; set; }
        public string message { get; set; }
        public int code { get; set; }
        public Data data { get; set; }

        public override string ToString()
        {
            return $"{{\"state\":{state},\"message\":\"{message}\",\"code\":{code},\"data\":{{\"pick_code\":\"{data.pick_code}\",\"file_size\":{data.file_size},\"file_id\":\"{data.file_size}\",\"thumb_url\":\"{data.thumb_url}\",\"sha1\":\"{data.sha1}\",\"aid\":{data.aid},\"file_name\":\"{data.file_name}\",\"cid\":\"{data.cid}\",\"is_video\":{data.is_video}}}";
        }
    }


    public class Data
    {
        public string pick_code { get; set; }
        public int file_size { get; set; }
        public string file_id { get; set; }
        public string thumb_url { get; set; }
        public string sha1 { get; set; }
        public int aid { get; set; }
        public string file_name { get; set; }
        public string cid { get; set; }
        public int is_video { get; set; }
    }



}
