using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Requests
{
    public partial class PlaylistExtenderRequest
    {
        [JsonProperty("playlistURI")]
        public string PlaylistUri { get; set; }

        [JsonProperty("trackSkipIDs")]
        public List<string> TrackSkipIDs { get; set; }
    }
}
