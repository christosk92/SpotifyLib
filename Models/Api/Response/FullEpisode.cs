using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLib.Enums;

namespace SpotifyLib.Models.Api.Response
{
    public class FullEpisode : GenericSpotifyItem
    {
        public string AudioPreviewUrl { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int DurationMs { get; set; }

        public bool Explicit { get; set; }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;


        public List<Image> Images { get; set; } = default!;

        public bool IsExternallyHosted { get; set; }

        public bool IsPlayable { get; set; }

        public List<string> Languages { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        public string ReleaseDate { get; set; } = default!;

        public string ReleaseDatePrecision { get; set; } = default!;

        public ResumePoint ResumePoint { get; set; } = default!;

        public SimpleShow Show { get; set; } = default!;

        [JsonConverter(typeof(StringEnumConverter))]
        public SpotifyType Type { get; set; }
    }
}


