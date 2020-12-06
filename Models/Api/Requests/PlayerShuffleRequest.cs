using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class PlayerShuffleRequest
    {
        /// <summary>
        /// true : Shuffle user’s playback false : Do not shuffle user’s playback.
        /// </summary>
        /// <value></value>
        [AliasAs("state")]
        public bool State { get; set; }
    }
}