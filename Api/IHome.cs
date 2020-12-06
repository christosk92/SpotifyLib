using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IHome
    {
        [Get("/v1/views/desktop-home")]
        Task<HomeResponse> GetHome(HomeRequest request);

        [Get("/v1/views/recently-played")]
        Task<RecentlyPlayedResponse> GetRecentlyPlayed(RecentlyPlayedRequest request);


        [Get("/v1/views/{id}")]
        Task<HomeResponseTrimmed> GetTagLineDetailed(string id, [AliasAs("locale")] string locale);

        [Get("/v1/views/{id}")]
        Task<HomeResponse> GetGenericView([AliasAs("country")] string? Country,

            [AliasAs("locale")] string? Locale,

            [AliasAs("market")] string? market,

            [AliasAs("timestamp")] string? timestamp,

            [AliasAs("platform")] string? platform,

            [AliasAs("content_limit")] int? content_limit,

            [AliasAs("offset")] int? offset,
            [AliasAs("limit")] int? limit,
            [AliasAs("types")] string? type,
            [AliasAs("image_style")] string? image_style,
            string id);
        [Get("/v1/views/{id}")]
        Task<HomeResponse> GetGenericView(string id, HomeRequest request);

    }
}