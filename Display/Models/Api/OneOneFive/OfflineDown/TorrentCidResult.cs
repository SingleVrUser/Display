using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class TorrentCidResult
{
    [JsonProperty("cid")]
    public long Cid { get; set; }
}