using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace SpotifyLib.Mercury
{
    public class MercuryAlbumRecommendations
    {
        [J("albumRecommendations")] public List<AlbumRecommendation> AlbumRecommendations { get; set; }
    }

    public class AlbumRecommendation
    {
        [J("uri")] public string Uri { get; set; }
    }
}
