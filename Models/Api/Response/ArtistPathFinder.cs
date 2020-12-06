using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class ArtistPathfinder
    {
        [J("data")] public Data Data { get; set; }
    }

    public partial class Data
    {
        [J("artist")] public Artist Artist { get; set; }
    }

    public partial class Artist
    {
        [J("visuals", NullValueHandling = N.Ignore)] public ArtistVisuals Visuals { get; set; }
    }
    public partial class ArtistVisuals
    {
        [J("headerImage", NullValueHandling = N.Ignore)] public AvatarImage HeaderImage { get; set; }
    }
    public partial class AvatarImage
    {
        [J("sources", NullValueHandling = N.Ignore)] public List<AvatarImageSource> Sources { get; set; }
    }
    public partial class AvatarImageSource
    {
        [J("url")] public string Url { get; set; }
        [J("width")] public long Width { get; set; }
        [J("height")] public long Height { get; set; }
    }

}