using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;

namespace SpotifyLib.Api
{
    [ResolvedSpClientEndpoint]
    public interface IStorageResolveService
    {
        /// <summary>
        /// Resolves the playback endpoints for a fileId.
        /// </summary>
        /// <param name="fileId">
        /// The file ID of the track.
        /// </param>
        /// <returns>
        [Get("/storage-resolve/files/audio/interactive/{fileId}?alt=json")]
        Task<StorageResolveResponseBody> ResolveFile(
            [AliasAs("fileId")] string fileId);

        [Get("/storage-resolve/files/audio/interactive_prefetch/{fileId}?alt=json")]
        Task<StorageResolveResponseBody> ResolvePreFetch(
            [AliasAs("fileId")] string fileId);
    }
}
