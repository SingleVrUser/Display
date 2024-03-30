using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

internal class FolderInfo
{
    [JsonProperty(propertyName: "name")]
    public string Name { get; set; }

    [JsonProperty(propertyName: "cid")]
    public string Cid { get; set; }

    [JsonProperty(propertyName: "pid")]
    public string Pid { get; set; }
}