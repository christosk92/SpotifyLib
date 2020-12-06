using System.Net.Http;
using System.Threading.Tasks;
using Connectstate;
using Refit;
using SpotifyLib.Attributes;

namespace SpotifyLib.Api
{

    [ResolvedSpClientEndpoint]
    public interface IConnectState
    {
        [Post("/connect-state/v1/player/command/from/{from}/to/{to}")]
        Task<HttpResponseMessage> TransferState(string from, string to, [Body]RemoteRequest request);


        [Put("/connect-state/v1/devices/{deviceId}")]
        Task<HttpResponseMessage> PutConnectState(
            [Header("X-Spotify-Connection-Id")] string conId,
            string deviceId,
            [Body] PutStateRequest req);
    }
}
