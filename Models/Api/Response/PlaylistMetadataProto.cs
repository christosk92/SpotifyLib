using System;
using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public partial class PlaylistMetadataProto
    {
        [J("revision")] public string Revision { get; set; }
        [J("length")] public long Length { get; set; }
        [J("attributes")] public Attributes Attributes { get; set; }
        [J("contents")] public Contents Contents { get; set; }
        [J("timestamp")] public string Timestamp { get; set; }
        [J("ownerUsername")] public string OwnerUsername { get; set; }
        [J("abuseReportingEnabled")] public bool AbuseReportingEnabled { get; set; }
    }

    public partial class Attributes
    {
        [J("name")] public string Name { get; set; }
        [J("description")] public string Description { get; set; }
        [J("picture")] public string Picture { get; set; }
        [J("collaborative")] public bool Collaborative { get; set; }
        [J("format")] public string Format { get; set; }
        [J("formatAttributes")] public List<FormatAttribute> FormatAttributes { get; set; }
        [J("pictureSize")] public List<PictureSize> PictureSize { get; set; }
    }

    public partial class FormatAttribute
    {
        [J("key")] public string Key { get; set; }
        [J("value")] public string Value { get; set; }
    }

    public partial class PictureSize
    {
        [J("targetName")] public string TargetName { get; set; }
        [J("url")] public Uri Url { get; set; }
    }

    public partial class Contents
    {
        [J("pos")] public long Pos { get; set; }
        [J("truncated")] public bool Truncated { get; set; }
    }
}
