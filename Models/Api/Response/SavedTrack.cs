using System;
using System.Collections;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class SavedTrack : GenericSpotifyItem,
        IComparable<SavedTrack>, IComparable
    {
        public SavedTrack()
        {
            base.IsSaved = true;
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [JsonProperty("added_at")]
        public DateTime AddedAt { get; set; }
        public FullTrack Track { get; set; } = default!;

        public string Name => Track.Name;
        public string Artist => Track.Artists[0].Name;
        public double Added => (AddedAt - epoch).TotalMilliseconds;
        public string Album => Track.Album.Name;
        public double Duration => Track.DurationMs;

        public int CompareTo(object obj)
        {
            return ((new CaseInsensitiveComparer()).Compare(Track.Id, (obj as SavedTrack).Track.Id));
        }

        public int CompareTo(SavedTrack other)
        {
            return ((new CaseInsensitiveComparer()).Compare(Track.Id, other.Track.Id));
        }
    }
}