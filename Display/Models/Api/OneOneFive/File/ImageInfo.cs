using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.File;

internal class ImageInfo
{
    [JsonProperty("state")]
    public bool State { get; set; }

    [JsonProperty("data")]
    public ImageData Data { get; set; }

    internal class ImageData
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("all_url")]
        public string[] UrlArray { get; set; }

        [JsonProperty("origin_url")]
        public string OriginUrl { get; set; }


        [JsonProperty("source_url")]
        public string SourceUrl { get; set; }


        [JsonProperty("file_name")]
        public string FileName { get; set; }


        [JsonProperty("file_sha1")]
        public string FileSha1 { get; set; }


        [JsonProperty("pick_code")]
        public string PickCode { get; set; }
    }
}