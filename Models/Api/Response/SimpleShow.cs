using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    public class SimpleShow : GenericSpotifyItem
    {
        public List<string> AvailableMarkets { get; set; } = default!;


        public string Description { get; set; } = default!;

        public bool Explicit { get; set; }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;


        public List<Image> Images { get; set; } = default!;

        public bool IsExternallyHosted { get; set; }


        public List<string> Languages { get; set; } = default!;

        public string MediaType { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Publisher { get; set; } = default!;

        public string Type { get; set; } = default!;
    }
}

