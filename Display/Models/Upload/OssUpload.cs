﻿namespace Display.Models.Upload
{
    public class OssUploadResult
    {
        public bool state { get; set; }
        public string message { get; set; }
        public int code { get; set; }
        public OssUploadResultData data { get; set; }

        public override string ToString()
        {
            return $"{{\"state\":{state},\"message\":\"{message}\",\"code\":{code},\"data\":{{\"pick_code\":\"{data.pick_code}\",\"file_size\":{data.file_size},\"file_id\":\"{data.file_size}\",\"thumb_url\":\"{data.thumb_url}\",\"sha1\":\"{data.sha1}\",\"aid\":{data.aid},\"file_name\":\"{data.file_name}\",\"cid\":\"{data.cid}\",\"is_video\":{data.is_video}}}";
        }
    }

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


}
