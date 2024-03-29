using System;
using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class DownInfo
{
    [JsonProperty(propertyName: "file_pickcode")]
    public string PickCode { get; set; }


    [JsonProperty(propertyName: "file_name")]
    public string FileName { get; set; }

    [JsonProperty(propertyName: "true_url")]
    public string TrueUrl { get; set; }

    [JsonProperty(propertyName: "ua")]
    public string Ua { get; set; }

    [JsonProperty(propertyName: "add_time")]
    public long AddTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
}