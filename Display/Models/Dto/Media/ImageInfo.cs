using Newtonsoft.Json;

namespace Display.Models.Dto.Media;

internal class ImageInfo
{
    [JsonProperty(propertyName: "state")]
    public bool State { get; set; }

    [JsonProperty(propertyName: "data")]
    public ImageData Data { get; set; }

    internal class ImageData
    {
        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "all_url")]
        public string[] UrlArray { get; set; }

        [JsonProperty(propertyName: "url")]
        public string OriginUrl { get; set; }


        [JsonProperty(propertyName: "source_url")]
        public string SourceUrl { get; set; }


        [JsonProperty(propertyName: "file_name")]
        public string FileName { get; set; }


        [JsonProperty(propertyName: "file_sha1")]
        public string FileSha1 { get; set; }


        [JsonProperty(propertyName: "pick_code")]
        public string PickCode { get; set; }
    }
}