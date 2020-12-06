using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLib.Enums;

namespace SpotifyLib.Models.Api.Response
{
	public class FullTrack : GenericSpotifyItem, IComparable<FullTrack>, IComparable
    {
        public event EventHandler<FullTrack> RequestPlay;
        public bool IsSaved { get; set; }
        public SimpleAlbum Album { get; set; } = default!;
        public List<SimpleArtist> Artists { get; set; } = default!;
        public List<string> AvailableMarkets { get; set; } = default!;
        public int DiscNumber { get; set; }
        [JsonProperty("duration_ms")]
        public long DurationMs { get; set; }
        public bool Explicit { get; set; }
        public Dictionary<string, string> ExternalIds { get; set; } = default!;
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        public bool IsPlayable { get; set; }
        public Dictionary<string, string> Restrictions { get; set; } = default!;
        [JsonProperty("name")]
        public string Name { get; set; } = default!;
        public int Popularity { get; set; }
        public string PreviewUrl { get; set; } = default!;
        public int TrackNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SpotifyType Type { get; set; }

        [JsonIgnore]
        public DateTime AddedAt { get; set; }
        public bool IsLocal { get; set; }
        public int CompareTo(object obj)
        {
            return ((new CaseInsensitiveComparer()).Compare(Id, (obj as FullTrack).Id));
        }

        public int CompareTo(FullTrack other)
        {
            return ((new CaseInsensitiveComparer()).Compare(Id, other.Id));
        }
    }
}


