using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Paging;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface ILibrary
    {
		/// <summary>
		/// Remove one or more albums from the current user’s ‘Your Music’ library.
		/// </summary>
		/// <param name="request">The request-model which contains required and optional parameters.</param>
		/// <remarks>
		/// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-remove-albums-user
		/// </remarks>
		/// <returns></returns>
		[Delete("/v1/me/albums")]
		Task<bool> RemoveAlbums(LibraryRemoveAlbumsRequest request);

        /// <summary>
        /// Check if one or more tracks is already saved in the current Spotify user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-check-users-saved-tracks
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/tracks/contains")]
        Task<List<bool>> CheckTracks(LibraryCheckTracksRequest request);

        [Get("/v1/me/albums/contains")]
        Task<List<bool>> CheckAlbums(LibraryCheckTracksRequest request);
        /// <summary>
        /// Get a list of the songs saved in the current Spotify user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-users-saved-tracks
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/tracks")]
        Task<Paging<SavedTrack>> GetTracks(LibraryTracksRequest request, [AliasAs("market")] string market = "from_token");

        /// <summary>
        /// Get a list of the albums saved in the current Spotify user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-users-saved-albums
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/albums")]
        Task<Paging<SavedAlbum>> GetAlbums(LibraryAlbumsRequest request, [AliasAs("market")] string market = "from_token");


        /// <summary>
        /// Save one or more tracks to the current user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-save-tracks-user
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/tracks")]
        Task SaveTracks([AliasAs("ids")] List<string> ids);

        /// <summary>
        /// Remove one or more tracks from the current user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-remove-tracks-user
        /// </remarks>
        /// <returns></returns>
        [Delete("/v1/me/tracks")]
        Task RemoveTracks([AliasAs("ids")] List<string> ids);
    }
}