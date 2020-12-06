using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SpotifyLib.Attributes;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IPlayer
	{
        /// <summary>
		/// Get information about the user’s current playback state, including track or episode, progress, and active device.
		/// </summary>
		/// <remarks>
		/// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-information-about-the-users-current-playback
		/// </remarks>
		/// <returns></returns>
		[Get("/v1/me/player")]
		Task<CurrentlyPlayingContext> GetCurrentPlayback([AliasAs("locale")] string locale);

		/// <summary>
		/// Get information about the user’s current playback state, including track or episode, progress, and active device.
		/// </summary>
		/// <param name="request">The request-model which contains required and optional parameters.</param>
		/// <remarks>
		/// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-information-about-the-users-current-playback
		/// </remarks>
		/// <returns></returns>
        [Get("/v1/me/player")]
		Task<CurrentlyPlayingContext> GetCurrentPlayback([AliasAs("locale")] string locale, PlayerCurrentPlaybackRequest request);

        /// <summary>
		/// Get information about a user’s available devices.
		/// </summary>
		/// <remarks>
		/// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-a-users-available-devices
		/// </remarks>
		/// <returns></returns>
	    [Get("/v1/me/player/devices")]
        Task<DeviceResponse> GetAvailableDevices();

        [Put("/v1/me/player/seek")]
        Task Seek([AliasAs("position_ms")] double position);

        /// <summary>
        /// Pause playback on the user’s account.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-pause-a-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/pause")]
        Task Pause();

        /// <summary>
        /// Start a new context or resume current playback on the user’s active device.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-start-a-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/play")]
        Task Resume();

        /// <summary>
        /// Skips to next track in the user’s queue.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-skip-users-playback-to-next-track
        /// </remarks>
        /// <returns></returns>
		[Post("/v1/me/player/next")]
        Task SkipNext();

        /// <summary>
        /// Skips to previous track in the user’s queue.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-skip-users-playback-to-previous-track
        /// </remarks>
        /// <returns></returns>
		[Post("/v1/me/player/previous")]
        Task SkipPrevious();



        /// <summary>
        /// Toggle shuffle on or off for user’s playback.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-toggle-shuffle-for-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/shuffle")]
        Task<HttpResponseMessage> SetShuffle([AliasAs("state")] bool state);

        /// <summary>
        /// Set the repeat mode for the user’s playback. Options are repeat-track, repeat-context, and off.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-set-repeat-mode-on-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/repeat")]
        Task SetRepeat([AliasAs("state")] string state);

        /// <summary>
        /// Set the repeat mode for the user’s playback. Options are repeat-track, repeat-context, and off.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-set-repeat-mode-on-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/volume")]
        Task SetVolume([AliasAs("volume_percent")] int state);


        /// <summary>
        /// Add an item to the end of the user’s current playback queue.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-add-to-queue
        /// </remarks>
        /// <returns></returns>
        [Post("/v1/me/player/queue")]
        Task<bool> AddToQueue([AliasAs("uri")]string uri);
    }
}