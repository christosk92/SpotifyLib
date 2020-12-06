using System;

namespace SpotifyLib.Models.Api.Response
{
    public class SavedAlbum : GenericSpotifyItem
    {
        public DateTime AddedAt { get; set; }
        public FullAlbum Album { get; set; } = default!;
    }
}