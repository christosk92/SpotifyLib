using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class Parser<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
