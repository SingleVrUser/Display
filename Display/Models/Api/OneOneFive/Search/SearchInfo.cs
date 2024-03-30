using Display.Models.Api.OneOneFive.File;
using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.Search;

internal class SearchInfo
{
    [JsonProperty(propertyName: "count")]
    public int Count { get; set; }

    [JsonProperty(propertyName: "data")]
    public SearchDatum[] Data { get; set; }

    [JsonProperty(propertyName: "cost_time_es")]
    public float CostTimeEs { get; set; }

    [JsonProperty(propertyName: "file_count")]
    public int FileCount { get; set; }


    [JsonProperty(propertyName: "folder_count")]
    public int FolderCount { get; set; }

    [JsonProperty(propertyName: "folder")]
    public FolderInfo FolderInfo { get; set; }

    [JsonProperty(propertyName: "page_size")]
    public int PageSize { get; set; }

    [JsonProperty(propertyName: "offset")]
    public int Offset { get; set; }

    [JsonProperty(propertyName: "is_asc")]
    public int IsAsc { get; set; }


    [JsonProperty(propertyName: "order")]
    public string Order { get; set; }

    [JsonProperty(propertyName: "suffix")]
    public string Suffix { get; set; }

    [JsonProperty(propertyName: "fc_mix")]
    public int FcMix { get; set; }

    [JsonProperty(propertyName: "type")]
    public int Type { get; set; }

    [JsonProperty(propertyName: "state")]
    public bool State { get; set; }

    [JsonProperty(propertyName: "error")]
    public string Error { get; set; }

    [JsonProperty(propertyName: "errCode")]
    public int ErrCode { get; set; }
}