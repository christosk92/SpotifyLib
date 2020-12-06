using System.Threading;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Mercury;

namespace SpotifyLib.Api
{
    [ResolvedSpClientEndpoint]
    public interface ISearch
    {
        [Get("/searchview/km/v4/search/{query}")]
        Task<MercurySearchResponse> Search(string query,
            [AliasAs("limit")] int limit,
            [AliasAs("imageSize")] string imageSize,
            [AliasAs("country")] string country,
            [AliasAs("username")] string username,
            [AliasAs("locale")] string locale, 
            CancellationToken cancellationToken,
            [AliasAs("catalogue")] string catalogue = "",
            [AliasAs("entityVersion")] int entityVersion = 2);
    }
}
