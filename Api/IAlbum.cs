using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IAlbum
    {
        /// <summary>
        /// Get Spotify catalog information for multiple albums identified by their Spotify IDs.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-multiple-albums
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/albums")]
        Task<AlbumsResponse> GetSeveral(AlbumsRequest request);
        /// <summary>
        /// Get Spotify catalog information for a single album.
        /// </summary>
        /// <param name="albumId">The Spotify ID of the album.</param>
        /// <param name="request">The request-model which contains required and optional parameters</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-an-album
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/albums/{albumId}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
        Task<FullAlbum> Get(string albumId, [AliasAs("market")]string market = "from_token");
    }
}