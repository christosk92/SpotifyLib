using System;
using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace SpotifyLib.Models.Api.Response
{
    public partial class SpotifyBrowse
    {
        [J("content")] public SpotifyBrowseContent Content { get; set; }
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

    public partial class SpotifyBrowseContent
    {
        [J("href")] public Uri Href { get; set; }
        [J("items")] public List<PurpleItem> Items { get; set; }
        [J("limit")] public long Limit { get; set; }
        [J("next")] public object Next { get; set; }
        [J("offset")] public long Offset { get; set; }
        [J("previous")] public object Previous { get; set; }
        [J("total")] public long Total { get; set; }
    }
    public partial class CustomFields
    {
    }
}