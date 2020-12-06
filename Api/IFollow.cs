using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IFollow
    {
        /// <summary>
        /// Get the current user’s followed artists.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-followed
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/following?type=artist")]
        Task<FollowedArtistsResponse> OfCurrentUser(FollowOfCurrentUserRequest request, 
            [AliasAs("market")] string market = "from_token");

        [Get("/v1/me/following/contains")]
        Task<List<bool>> OfCurrentUser([AliasAs("type")] string type, [AliasAs("ids")] string ids);
    }
}
