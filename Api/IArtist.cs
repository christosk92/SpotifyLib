using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api-partner.spotify.com/")]
    public interface IArtist
    {
        [Get("/pathfinder/v1/query?operationName=queryArtistOverview&variables=%7B%22uri%22%3A%22spotify:artist:{id}%22%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22ddefd0ecde2c1cba2023fcff03f1b51c18077472803cfeb441a86a28dc6fb36b%22%7D%7D")]
        Task<ArtistPathfinder> GetArtist(string id);
    }
    [BaseUrl("https://api.spotify.com")]
    public interface IOpenIArtist
    {
        [Get("/v1/artists/{id}")]
        Task<FullArtist> GetArtist(string id);
    }
}
