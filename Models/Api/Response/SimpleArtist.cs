using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    public class SimpleArtist : GenericSpotifyItem
    {
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;


        public string Href { get; set; } = default!;


        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

    }
}

