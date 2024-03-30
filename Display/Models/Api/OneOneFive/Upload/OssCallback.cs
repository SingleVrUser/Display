using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class OssCallback
{
    [JsonProperty("callback")]
    public string Callback { get; set; }
    
    [JsonProperty("callback_var")]
    public string CallbackVar { get; set; }
}