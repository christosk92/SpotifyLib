using Newtonsoft.Json;
using SpotifyLib.Api;

namespace SpotifyLib.Models.Api.Response
{
    /// <summary>
    /// The body returned by <see cref="ISeektables.GetSeektable(string)"/>
    /// </summary>
    public class SeektableResponseBody
    {
        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("padding_samples")]
        public int PaddingSamples { get; set; }

        /// <summary>
        /// Used to get Challenge Data for DRM-enabled Playready playback
        /// </summary>
        [JsonProperty("pssh_playready")]
        public string PsshPlayready { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("pssh")]
        public string Pssh { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("encoder_delay_samples")]
        public string EncoderDelaySamples { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("timescale")]
        public int Timescale { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("index_range")]
        public int[] IndexRange { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("segments")]
        public int[][] Segments { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("seektable_version")]
        public string SeektableVersion { get; set; }

        /// <summary>
        /// Details unknown
        /// </summary>
        [JsonProperty("pssh_widevine")]
        public string PsshWidevine { get; set; }
    }
}
