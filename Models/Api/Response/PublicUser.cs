using System.Collections.Generic;
using SpotifyLib.Models.Mercury;

namespace SpotifyLib.Models.Api.Response
{
    public class PublicUser
    {
        public string DisplayName { get; set; } = default!;

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public Followers Followers { get; set; } = default!;

        public string Href { get; set; } = default!;

        public string Id { get; set; } = default!;

        public List<Cover> Images { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string Uri { get; set; } = default!;
    }
}

