using Newtonsoft.Json;
using Spotify.Player.Proto;

namespace SpotifyLib.Models.Api.Response
{
    public class CurrentlyPlayingContext
    {
        public Device Device { get; set; } = default!;

        [JsonProperty("repeat_state")]
        public string RepeatState { get; set; } = default!;

        [JsonProperty("shuffle_state")]
        public bool ShuffleState { get; set; }
            
        public Context Context { get; set; } = default!;

        public long Timestamp { get; set; }

        [JsonProperty("progress_ms")]
        public int ProgressMs { get; set; }

        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }

        [JsonConverter(typeof(PlayableItemConverter))]
        public IApiItem Item { get; set; } = default!;

        public string CurrentlyPlayingType { get; set; } = default!;
    }
}