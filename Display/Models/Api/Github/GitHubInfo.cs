using System;
using Newtonsoft.Json;

namespace Display.Models.Api.Github;

internal abstract class GitHubInfo
{
    public class ReleaseInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("assets_url")]
        public string AssetsUrl { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "tag_name", Required = Required.Default)]
        public string TagName { get; set; }

        [JsonProperty("target_commitish")]
        public string TargetCommitish { get; set; }
        
        [JsonProperty("name")]
        
        public string Name { get; set; }
        
        
        [JsonProperty("draft")]
        public bool Draft { get; set; }

        /// <summary>
        /// 是否是预发布版本
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "prerelease", Required = Required.Default)]
        public bool IsPreRelease { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// 发布时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "published_at", Required = Required.Default)]
        public DateTime PublishedTime { get; set; }
        
        [JsonProperty("assets")]
        public Asset[] Assets { get; set; }
        
        
        [JsonProperty("tarball_url")]
        public string TarballUrl { get; set; }
        
        [JsonProperty("zipball_url")]
        public string ZipBallUrl { get; set; }

        /// <summary>
        /// 更新内容
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body", Required = Required.Default)]
        public string UpdateContent { get; set; }
    }

    public class Author
    {
        [JsonProperty("login")]
        public string Login { get; set; }
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("node_id")]
        public string NodeId { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        
        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }
        
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }
        
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }
        
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }
        
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }
        
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }
        
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }
        
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }
        
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class Asset
    {
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("node_id")]
        public string NodeId { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("label")]
        public object Label { get; set; }
        
        [JsonProperty("uploader")]
        public Uploader Uploader { get; set; }
        
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
        
        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("size")]
        public int Size { get; set; }
        
        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    public class Uploader
    {
        
        [JsonProperty("login")]
        public string Login { get; set; }
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("node_id")]
        public string NodeId { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        
        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }
        
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }
        
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }
        
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }
        
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }
        
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }
        
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }
        
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }
        
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }
}
