using Newtonsoft.Json;

namespace SpotifyLib.Models.Mercury
{
    public partial class UserPresence
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("trackUri")]
        public string TrackUri { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("contextUri")]
        public string ContextUri { get; set; }

        [JsonProperty("contextIndex")]
        public long ContextIndex { get; set; }
    }
}
