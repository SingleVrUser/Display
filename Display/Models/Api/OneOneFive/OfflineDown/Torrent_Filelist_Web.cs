using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.OfflineDown;

public class TorrentFileListWeb
{
    [JsonProperty("size")]
    public long Size { get; set; }
    
    [JsonProperty("path")]
    public string Path { get; set; }
    
    [JsonProperty("wanted")]
    public int Wanted { get; set; }
}