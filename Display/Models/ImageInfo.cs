using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Models
{
    public class ImageInfo
    {
        public bool state { get; set; }
        public ImageData data { get; set; }

        public class ImageData
        {
            public string url { get; set; }
            public string[] all_url { get; set; }
            public string origin_url { get; set; }
            public string source_url { get; set; }
            public string file_name { get; set; }
            public string file_sha1 { get; set; }
            public string pick_code { get; set; }
        }
    }
}
