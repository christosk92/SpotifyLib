using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Mercury
{
    public class GroupedDiscCollection : List<DiscTrack>
    {
        public GroupedDiscCollection(int discNumber,
            IEnumerable<DiscTrack> items) : base(items)
        {
            this.Name = $"Disc {discNumber}";
        }
        public string Name { get; }
    }

    public class MercuryAlbum : GenericSpotifyItem
    {
        private List<Disc> _discs;

        [JsonIgnore]
        public string Description { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public long TrackCount { get; set; }

        [JsonProperty("discs")]
        public List<Disc> Discs
        {
            get => _discs;
            set { _discs = value; }
        }

        public long Month { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class MercuryAlbumArtist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class Cover
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class Track
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("playcount")]
        public long Playcount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("playable")]
        public bool Playable { get; set; }

        [JsonProperty("artists")]
        public List<TrackArtist> Artists { get; set; }
    }

    public partial class TrackArtist
    {

        [JsonProperty("image")]
        public Cover Image { get; set; }
    }

    public partial class Related
    {
        [JsonProperty("releases")]
        public List<Release> Releases { get; set; }
    }

    public partial class Release : GenericSpotifyItem
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public long TrackCount { get; set; }

        [JsonProperty("month")]
        public long Month { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }
    }
}