#nullable enable
using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    public class FullPlaylist : GenericSpotifyItem
    {
        public bool? Collaborative { get; set; }
        public string? Description { get; set; } = default!;
        public Dictionary<string, string>? ExternalUrls { get; set; } = default!;
        public Followers Followers { get; set; } = default!;
        public string? Href { get; set; } = default!;
        public List<Image>? Images { get; set; } = default!;
        public string? Name { get; set; } = default!;
        public PublicUser? Owner { get; set; } = default!;
        public bool? Public { get; set; }
        public string? SnapshotId { get; set; } = default!;

        public string? Type { get; set; } = default!;
    }
}