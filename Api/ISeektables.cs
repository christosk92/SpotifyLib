using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    /// <summary>
    /// A service that interacts with the seektables endpoint
    /// </summary>
    [BaseUrl("https://seektables.scdn.co")]
    public interface ISeektables
    {
        /// <summary>
        /// Gets the seektable for a file. Is used for native playback.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        [Get("/seektable/{fileId}.json")]
        Task<SeektableResponseBody> GetSeektable([AliasAs("fileId")] string fileId);
    }
}
