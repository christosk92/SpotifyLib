using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [ResolvedSpClientEndpoint]
    public interface IMetadata
    {
        [Get("/metadata/4/episode/{episodeId}")]
        Task<HttpContent> GetMetadataForEpisode(string episodeId);
        [Get("/metadata/4/track/{trackId}")]
        Task<MetadataTrack> GetMetadataForTrack(string trackId);

        [Get("/playlist/v2/playlist/{plistId}/metadata")]
        Task<PlaylistMetadataProto> GetMetadataForPlaylist(string plistId);
    }
}
