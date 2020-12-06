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

    public partial class RecentlyPlayedItem : GenericSpotifyItem
    {
        [J("collaborative", NullValueHandling = N.Ignore)] public bool? Collaborative { get; set; }
        [J("description", NullValueHandling = N.Ignore)] public string Description { get; set; }
        [J("external_urls")] public ExternalUrls ExternalUrls { get; set; }
        [J("href")] public Uri Href { get; set; }
        [J("images")] public List<Image> Images { get; set; }
        [J("name")] public string Name { get; set; }
        [J("owner", NullValueHandling = N.Ignore)] public Owner Owner { get; set; }
        [J("primary_color")] public object PrimaryColor { get; set; }
        [J("public")] public object Public { get; set; }
        [J("snapshot_id", NullValueHandling = N.Ignore)] public string SnapshotId { get; set; }
        [J("tracks", NullValueHandling = N.Ignore)] public Tracks Tracks { get; set; }
        [J("type")] public string Type { get; set; }
        [J("album_type", NullValueHandling = N.Ignore)] public string AlbumType { get; set; }
        [J("artists", NullValueHandling = N.Ignore)] public List<Artist> Artists { get; set; }
        [J("release_date", NullValueHandling = N.Ignore)] public string ReleaseDate { get; set; }
        [J("release_date_precision", NullValueHandling = N.Ignore)] public string ReleaseDatePrecision { get; set; }
        [J("total_tracks", NullValueHandling = N.Ignore)] public long? TotalTracks { get; set; }
        [J("followers", NullValueHandling = N.Ignore)] public Followers Followers { get; set; }
        [J("genres", NullValueHandling = N.Ignore)] public List<string> Genres { get; set; }
        [J("popularity", NullValueHandling = N.Ignore)] public long? Popularity { get; set; }
    }
    public partial class CustomFields
    {
    }
}