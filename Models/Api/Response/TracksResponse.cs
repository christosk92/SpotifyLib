using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    public class TracksResponse
    {
        public List<FullTrack> Tracks { get; set; } = default!;
    }
}

