using System;
using Newtonsoft.Json;

namespace Display.Models.Dto;

public class GitHubInfo
{
    public class ReleaseInfo
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "assets_url")]
        public string AssetsUrl { get; set; }

        [JsonProperty(propertyName: "upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty(propertyName: "html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty(propertyName: "author")]
        public Author Author { get; set; }

        [JsonProperty(propertyName: "node_id")]
        public string NodeId { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "tag_name", Required = Required.Default)]
        public string TagName { get; set; }

        public string target_commitish { get; set; }
        public string name { get; set; }
        public bool draft { get; set; }

        /// <summary>
        /// 是否是预发布版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "prerelease", Required = Required.Default)]
        public bool IsPreRelease { get; set; }

        public DateTime created_at { get; set; }


        /// <summary>
        /// 发布时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "published_at", Required = Required.Default)]
        public DateTime PublishedTime { get; set; }
        public Asset[] assets { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }

        /// <summary>
        /// 更新内容
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body", Required = Required.Default)]
        public string UpdateContent { get; set; }
    }

    public class Author
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    public class Asset
    {
        public string url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string name { get; set; }
        public object label { get; set; }
        public Uploader uploader { get; set; }
        public string content_type { get; set; }
        public string state { get; set; }
        public int size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string browser_download_url { get; set; }
    }

    public class Uploader
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }
}
