using System;
using System.Collections.Generic;

using System.Globalization;
using Base62;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace SpotifyLib.Models.Api.Response
{
    public partial class MetadataTrack
    {
        [JsonProperty("gid")] public string Gid { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("album")] public MetadataAlbum Album { get; set; }

        [JsonProperty("artist")] public List<MetadataArtist> Artist { get; set; }

        [JsonProperty("number")] public long Number { get; set; }

        [JsonProperty("disc_number")] public long DiscNumber { get; set; }

        [JsonProperty("duration")] public long Duration { get; set; }

        [JsonProperty("popularity")] public long Popularity { get; set; }

        [JsonProperty("external_id")] public List<ExternalId> ExternalId { get; set; }

        [JsonProperty("file")] public List<File> File { get; set; }

        [JsonProperty("preview")] public List<File> Preview { get; set; }

        [JsonProperty("has_lyrics")] public bool HasLyrics { get; set; }

        [JsonProperty("licensor")] public Licensor Licensor { get; set; }

        [JsonProperty("language_of_performance")]
        public List<string> LanguageOfPerformance { get; set; }

        [JsonProperty("localized_name")] public List<LocalizedName> LocalizedName { get; set; }

        [JsonProperty("original_audio")] public Licensor OriginalAudio { get; set; }

        [JsonProperty("original_title")] public string OriginalTitle { get; set; }

        [JsonProperty("version_title")] public string VersionTitle { get; set; }

        [JsonProperty("artist_with_role")] public List<ArtistWithRole> ArtistWithRole { get; set; }
    }

    public partial class MetadataAlbum : GenericSpotifyItem
    {
        private string _gid;
        [JsonProperty("gid")]
        public string Gid
        {
            get => _gid;
            set
            {
                _gid = value;
                base.Uri = $"spotify:album:{value.ToBase62(true)}";
            }
        }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("artist")] public List<Artist> Artist { get; set; }

        [JsonProperty("label")] public string Label { get; set; }

        [JsonProperty("date")] public Date Date { get; set; }

        [JsonProperty("cover_group")] public CoverGroup CoverGroup { get; set; }

        [JsonProperty("localized_name")] public List<LocalizedName> LocalizedName { get; set; }
    }

    public partial class MetadataArtist
    {
        [JsonProperty("gid")] public string Gid { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
    }

    public partial class CoverGroup
    {
        [JsonProperty("image")] public List<metadataImage> Image { get; set; }
    }

    public partial class metadataImage
    {
        [JsonProperty("file_id")] public string FileId { get; set; }

        [JsonProperty("size")] public string Size { get; set; }

        [JsonProperty("width")] public long Width { get; set; }

        [JsonProperty("height")] public long Height { get; set; }
    }

    public partial class Date
    {
        [JsonProperty("year")] public long Year { get; set; }

        [JsonProperty("month")] public long Month { get; set; }

        [JsonProperty("day")] public long Day { get; set; }
    }

    public partial class LocalizedName
    {
        [JsonProperty("language")] public string Language { get; set; }

        [JsonProperty("value")] public string Value { get; set; }
    }

    public partial class ArtistWithRole
    {
        [JsonProperty("artist_gid")] public string ArtistGid { get; set; }

        [JsonProperty("artist_name")] public string ArtistName { get; set; }

        [JsonProperty("role")] public string Role { get; set; }
    }

    public partial class ExternalId
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("id")] public string Id { get; set; }
    }

    public partial class File
    {
        [JsonProperty("file_id")] public string FileId { get; set; }

        [JsonProperty("format")] public string Format { get; set; }
    }

    public partial class Licensor
    {
        [JsonProperty("uuid")] public string Uuid { get; set; }
    }

    public partial class MetadataTrack
    {
        public static MetadataTrack FromJson(string json) =>
            JsonConvert.DeserializeObject<MetadataTrack>(json, SpotifyLib.Models.Api.Response.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this MetadataTrack self) =>
            JsonConvert.SerializeObject(self, SpotifyLib.Models.Api.Response.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            },
        };
    }
}