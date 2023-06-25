using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Models
{
    public class Upload115Result
    {
        public string request { get; set; }
        public int status { get; set; }
        public int statuscode { get; set; }
        public string statusmsg { get; set; }
        public string pickcode { get; set; }
        public string target { get; set; }
        public string version { get; set; }
        public string sign_key { get; set; }
        public string sign_check { get; set; }
        public string bucket { get; set; }

        [JsonProperty(propertyName: "object")]
        public string Object { get; set; }
        public Callback callback { get; set; }
    }

    public class Callback
    {
        public string callback { get; set; }
        public string callback_var { get; set; }
    }

    public class OssToken
    {
        public string StatusCode { get; set; }
        public string AccessKeySecret { get; set; }
        public string SecurityToken { get; set; }
        public DateTime Expiration { get; set; }
        public string AccessKeyId { get; set; }
    }
}
