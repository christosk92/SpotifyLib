using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models;
using SpotifyLib.Models.Api;
using SpotifyLib.Models.Api.Paging;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    /// <summary>
    /// Endpoints for retrieving information about a user’s playlists and for managing a user’s playlists.
    /// </summary>
    [BaseUrl("https://api.spotify.com")]
    public interface IPlaylist
    {
        /// Remove one or more items from a user’s playlist.
        /// </summary>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-remove-tracks-playlist
        /// </remarks>
        /// <returns></returns>
        [Delete("/v1/playlists/{playlistId}/tracks")]
        Task<SnapshotResponse> RemoveItems(string playlistId, PlaylistRemoveItemsRequest request);

        /// <summary>
        /// Add one or more items to a user’s playlist.
        /// </summary>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="uris">Comma seperated track uris.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-add-tracks-to-playlist
        /// </remarks>
        /// <returns></returns>
        [Post("/v1/playlists/{playlistId}/tracks")]
        Task<SnapshotResponse> AddItems(string playlistId, [AliasAs("uris")] string uris);

        /// <summary>
        /// Get the current image associated with a specific playlist.
        /// </summary>
        /// <param name="playlist_id">The Spotify ID for the playlist.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-playlist-cover
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/playlists/{playlist_id}/images")]
        Task<List<Image>> GetCovers(string playlist_id);

        /// <summary>
        /// Get the playlist
        /// </summary>
        /// <param name="playlist_id">The Spotify ID for the playlist.</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/playlists/{playlist_id}")]
        Task<FullPlaylist> GetPlaylist(string playlist_id, PlaylistRequest request);

        [Get("/v1/me/playlists")]
        Task<Paging<SimplePlaylist>> GetUserPlaylists();

        [Get("/v1/playlists/{plistId}/tracks")]
        Task<Paging<PlaylistTrack<GenericSpotifyItem>>> GetPlaylistTracks(string plistId, PlaylistTracksRequest request);

        /// <summary>
        /// Reorder an item or a group of items in a playlist.
        /// </summary>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-reorder-playlists-tracks
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/playlists/{playlist_id}/tracks")]
        Task<SnapshotResponse> ReorderItems(string playlist_id,
            [Body] PlaylistReorderItemsRequest req);
    }
}