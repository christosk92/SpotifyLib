using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class ArtistLol
    {
        [JsonProperty("artist", NullValueHandling = NullValueHandling.Ignore)]
        public ArtistColors ArtistColors { get; set; }
    }

    public class ArtistColors
    {
        [JsonProperty("visuals", NullValueHandling = NullValueHandling.Ignore)]
        public ArtistColorVisuals Visuals { get; set; }
    }

    public class ArtistColorVisuals
    {
        [JsonProperty("headerImage", NullValueHandling = NullValueHandling.Ignore)]
        public ArtistColorHeaderImage HeaderImage { get; set; }
    }

    public partial class ArtistColorHeaderImage
    {
        [JsonProperty("colors", NullValueHandling = NullValueHandling.Ignore)]
        public List<SpotifyColor> Colors { get; set; }
    }

    public partial class SpotifyColor
    {
        [JsonProperty("hex", NullValueHandling = NullValueHandling.Ignore)]
        public string Hex { get; set; }

        [JsonProperty("preset", NullValueHandling = NullValueHandling.Ignore)]
        public string Preset { get; set; }
    }
}
