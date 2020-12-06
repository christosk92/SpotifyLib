using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api-partner.spotify.com")]
    public interface IPathFinder
    {
        [Get(
            "/pathfinder/v1/query?operationName=artistPageHeaderColor&variables=%7B%22uri%22%3A%22spotify:artist:{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%220222b5ea4efad3c2e1328dfe9fae90824483541c1b4dde434a532e39ddf314c8%22%7D%7D")]
        Task<Parser<ArtistLol>> GetColors(string id);

        [Get(
            "/pathfinder/v1/query?operationName=queryAlbumTracks&variables=%7B%22uri%22%3A%22spotify:album:{id}%22%2C%22offset%22%3A0%2C%22limit%22%3A300%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%223ea563e1d68f486d8df30f69de9dcedae74c77e684b889ba7408c589d30f7f2e%22%7D%7D")]
        Task<PathFinderAlbumTracks> QueryAlbumTracks(string id);
    }
}
