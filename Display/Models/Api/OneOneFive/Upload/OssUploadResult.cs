using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Upload;

public class OssUploadResult
{
    [JsonProperty("state")]
    public bool State { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("data")]
    public OssUploadResultData Data { get; set; }

    public override string ToString()
    {
        return $"{{\"state\":{State},\"message\":\"{Message}\",\"code\":{Code},\"data\":{{\"pick_code\":\"{Data.PickCode}\",\"file_size\":{Data.FileSize},\"file_id\":\"{Data.FileSize}\",\"thumb_url\":\"{Data.ThumbUrl}\",\"sha1\":\"{Data.Sha1}\",\"aid\":{Data.Aid},\"file_name\":\"{Data.FileName}\",\"cid\":\"{Data.Cid}\",\"is_video\":{Data.IsVideo}}}";
    }
}