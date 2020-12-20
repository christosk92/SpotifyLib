using System;
using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class RecentlyPlayedResponse
    {
        [J("content")] public Content Content { get; set; }
        [J("custom_fields")] public CustomFields CustomFields { get; set; }
        [J("external_urls")] public object ExternalUrls { get; set; }
        [J("href")] public Uri Href { get; set; }
        [J("id")] public string Id { get; set; }
        [J("images")] public List<object> Images { get; set; }
        [J("name")] public string Name { get; set; }
        [J("rendering")] public string Rendering { get; set; }
        [J("tag_line")] public object TagLine { get; set; }
        [J("type")] public string Type { get; set; }
    }

    public partial class Content
    {
        [J("href")] public Uri Href { get; set; }
        [J("items")] public List<RecentlyPlayedItem> Items { get; set; }
        [J("limit")] public long Limit { get; set; }
        [J("next")] public Uri Next { get; set; }
        [J("offset")] public long Offset { get; set; }
        [J("previous")] public object Previous { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class RecentlyPlayedItem : FluffyItem
    {

    }
    public partial class CustomFields
    {
    }
}