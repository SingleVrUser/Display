using Display.Models.Entities.Upload;
using Newtonsoft.Json;

namespace Display.Models.Upload;

public class FastUploadResult
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