using Refit;
using SpotifyLib.Attributes;

namespace SpotifyLib.Models.Api.Requests
{
    public class PlayerSetRepeatRequest
    {
        public PlayerSetRepeatRequest(RepeatState state)
        {
            Ensure.ArgumentNotNull(state, nameof(state));

            StateParam = state;
        }

        /// <summary>
        /// The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.
        /// </summary>
        /// <value></value>
        [AliasAs("device_id")]
        public string? DeviceId { get; set; }

        /// <summary>
        /// track, context or off. track will repeat the current track. context will repeat the current context.
        /// off will turn repeat off.
        /// </summary>
        /// <value></value>
        [AliasAs("state")]
        public RepeatState StateParam { get; }

        public enum RepeatState
        {
            [String("track")]
            Track,

            [String("context")]
            Context,

            [String("off")]
            Off
        }
    }
}