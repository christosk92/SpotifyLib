using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface ITrack
    {
        /// <summary>
        /// Get Spotify catalog information for a single track identified by its unique Spotify ID.
        /// </summary>
        /// <param name="trackId">The Spotify ID for the track.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        [Get("/v1/tracks/{trackId}")]
        Task<FullTrack> GetTrack(string trackId, [AliasAs("market")] string market = "from_token");

        /// <summary>
        /// Get Spotify catalog information for multiple tracks based on their Spotify IDs.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-several-tracks
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/tracks")]
        Task<TracksResponse> GetSeveral([AliasAs("ids")] List<string> ids);

    }
}
