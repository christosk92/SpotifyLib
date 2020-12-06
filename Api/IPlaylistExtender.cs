using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    /// <summary>
    /// Playlist extender.
    /// </summary>
    [ResolvedSpClientEndpoint]
    public interface IPlaylistExtender
    {
        /// <summary>
        /// Gets recommended tracks for a playlist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/playlistextender/extendp/")]
        Task<PlaylistExtenderResult> ExtendPlaylist([Body] PlaylistExtenderRequest request);
    }
}
