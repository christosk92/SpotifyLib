using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IUserService
    {
        /// <summary>
        /// Get detailed profile information about the current user (including the current user’s username).
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-current-users-profile
        /// </remarks>
        /// <exception cref="APIUnauthorizedException">
        /// Thrown if the client is not authenticated.
        /// </exception> 
        [Get("/v1/me")]
        Task<PrivateUser> Current();

        /// <summary>
        /// Get public profile information about a Spotify user.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-users-profile
        /// </remarks>
        /// <exception cref="APIUnauthorizedException">
        /// Thrown if the client is not authenticated.
        /// </exception>
        [Get("/v1/users/{userId}")]
        Task<PublicUser> Get(string userId);
    }
}