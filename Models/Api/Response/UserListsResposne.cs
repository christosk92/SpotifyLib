using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class UserListsResponse
    {
        [J("revision")] public string Revision { get; set; }
        [J("length")] public long Length { get; set; }
        [J("attributes")] public UserListsResposneAttributes Attributes { get; set; }
        [J("contents")] public Contents Contents { get; set; }
        [J("timestamp")] public string Timestamp { get; set; }
    }

    public partial class UserListsResposneAttributes
    {
    }

    public partial class Contents
    {
        [J("items")] public List<Item> Items { get; set; }
        [J("metaItems")] public List<MetaItem> MetaItems { get; set; }
    }

    public partial class Item
    {
        [J("uri")] public string Uri { get; set; }
        [J("attributes")] public ItemAttributes Attributes { get; set; }
    }

    public partial class ItemAttributes
    {
        [J("timestamp")] public string Timestamp { get; set; }
        [J("seenAt", NullValueHandling = N.Ignore)] public string SeenAt { get; set; }
        [J("public", NullValueHandling = N.Ignore)] public bool? Public { get; set; }
    }

    public partial class MetaItem
    {
        [J("revision", NullValueHandling = N.Ignore)] public string Revision { get; set; }
        [J("attributes", NullValueHandling = N.Ignore)] public MetaItemAttributes Attributes { get; set; }
        [J("length", NullValueHandling = N.Ignore)] public long? Length { get; set; }
        [J("timestamp", NullValueHandling = N.Ignore)] public string Timestamp { get; set; }
        [J("ownerUsername", NullValueHandling = N.Ignore)] public string OwnerUsername { get; set; }
    }

    public partial class MetaItemAttributes
    {
        [J("name")] public string Name { get; set; }
        [J("description", NullValueHandling = N.Ignore)] public string Description { get; set; }
        [J("picture", NullValueHandling = N.Ignore)] public string Picture { get; set; }
        [J("collaborative", NullValueHandling = N.Ignore)] public bool? Collaborative { get; set; }
        [J("format", NullValueHandling = N.Ignore)] public string Format { get; set; }
        [J("formatAttributes", NullValueHandling = N.Ignore)] public List<FormatAttribute> FormatAttributes { get; set; }
        [J("pictureSize", NullValueHandling = N.Ignore)] public List<PictureSize> PictureSize { get; set; }
    }
}
