using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpotifyLib.Models.Api.Response
{
    public class SimpleTrack : GenericSpotifyItem
    {
        public List<SimpleArtist> Artists { get; set; } = default!;
        public List<string> AvailableMarkets { get; set; } = default!;
        public int DiscNumber { get; set; }
        public int DurationMs { get; set; }
        public bool Explicit { get; set; }
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        public bool IsPlayable { get; set; }
        public string Name { get; set; } = default!;
        public string PreviewUrl { get; set; } = default!;
        public int TrackNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType Type { get; set; }
    }
}
