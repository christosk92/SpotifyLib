namespace SpotifyLib.Models.Api.Requests
{
    public class PlayingChangedRequest
    {
        public string ItemUri { get; set; }
        public string ContextUri { get; set; }
        public bool? IsPaused { get; set; }
        public bool? IsPlaying { get; set; }
    }
}

