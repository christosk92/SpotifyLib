using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace SpotifyLib.Helpers
{
    public struct ApResolver
    {
        private string _resolvedDealer;
        private string _resolvedSpClient;
        public async System.Threading.Tasks.Task<string> GetClosestDealerAsync()
        {
            if (!string.IsNullOrEmpty(_resolvedDealer))
                return _resolvedDealer;
            //https://apresolve.spotify.com/?type=dealer
            using var client = new HttpClient();
            var dealers = await client.GetStringAsync("https://apresolve.spotify.com/?type=dealer");
            var x = JObject.Parse(dealers)["dealer"];
            _resolvedDealer = "https://" + x.First;
            return _resolvedDealer;
        }

        public async System.Threading.Tasks.Task<string> GetClosestSpClient()
        {
            if (!string.IsNullOrEmpty(_resolvedSpClient))
                return _resolvedSpClient;
            //https://apresolve.spotify.com/?type=spclient
            using var client = new HttpClient();
            var spclients = await client.GetStringAsync("https://apresolve.spotify.com/?type=spclient");
            var x = JObject.Parse(spclients)["spclient"];
            _resolvedSpClient = "https://" + x.First;
            return _resolvedSpClient;
        }
    }
}