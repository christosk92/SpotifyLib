using SpotifyLib.Models.Api.Paging;

namespace SpotifyLib.Models.Api.Response
{
    public class FollowedArtistsResponse
    {
        public CursorPaging<FullArtist, FollowedArtistsResponse> Artists { get; set; } = default!;
    }
}
