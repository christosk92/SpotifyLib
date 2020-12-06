using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    public class Melody
    {
        public long timestamp { get; set; }
    }

    [ResolvedSpClientEndpoint]
    public interface IMelody
    {
        [Get("/melody/v1/time")]
        Task<Melody> GetTime();

        [Get("/playlist/v2/user/{username}/rootlist")]
        [Headers("Accept: application/json")]
        Task<UserListsResponse> GetUserLists(string username, [AliasAs("decorate")]string decorate);
    }
}
