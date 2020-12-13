using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    public partial class ArtistQuickHeader
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("artist")]
        public ArtistQuick Artist { get; set; }
    }

    public partial class ArtistQuick
    {
        [JsonProperty("visuals")]
        public ArtistVisualsQuick Visuals { get; set; }
    }
    public partial class ArtistVisualsQuick
    {
        [JsonProperty("headerImage")]
        public RImage HeaderImage { get; set; }
    }
    public partial class RImage
    {
        [JsonProperty("sources")]
        public List<ItemSource> Sources { get; set; }
        [JsonProperty("extractedColors")]
        public ExtractedColors ExtractedColors { get; set; }
    }
    public partial class ExtractedColors
    {
        [JsonProperty("colorRaw")]
        public ColorRaw ColorRaw { get; set; }
    }
    public partial class ColorRaw
    {
        [JsonProperty("hex")]
        public string Hex { get; set; }
    }
    public partial class ItemSource
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }

        [JsonProperty("height")]
        public long? Height { get; set; }
    }

    [BaseUrl("https://api-partner.spotify.com")]
    public interface IPathFinder
    {
        [Get(
            "/pathfinder/v1/query?operationName=queryArtistOverview&variables=%7B%22uri%22%3A%22spotify:artist:{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%2253f2fcff0a0f47530d71f576113ed9db94fc3ccd1e8c7420c0852b828cadd2e0%22%7D%7D")]
        Task<ArtistQuickHeader> GetArtistHeaderOnly(string id, CancellationToken ct);
        [Get(
            "/pathfinder/v1/query?operationName=queryArtistOverview&variables=%7B%22uri%22%3A%22spotify:artist:{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%2253f2fcff0a0f47530d71f576113ed9db94fc3ccd1e8c7420c0852b828cadd2e0%22%7D%7D")]
        Task<ArtistQuickHeader> GetArtistHeaderOnly(string id);
        [Get(
            "/pathfinder/v1/query?operationName=artistPageHeaderColor&variables=%7B%22uri%22%3A%22spotify:artist:{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%220222b5ea4efad3c2e1328dfe9fae90824483541c1b4dde434a532e39ddf314c8%22%7D%7D")]
        Task<Parser<ArtistLol>> GetColors(string id);

        [Get(
            "/pathfinder/v1/query?operationName=queryAlbumTracks&variables=%7B%22uri%22%3A%22spotify:album:{id}%22%2C%22offset%22%3A0%2C%22limit%22%3A300%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%223ea563e1d68f486d8df30f69de9dcedae74c77e684b889ba7408c589d30f7f2e%22%7D%7D")]
        Task<PathFinderAlbumTracks> QueryAlbumTracks(string id);
    }
}
