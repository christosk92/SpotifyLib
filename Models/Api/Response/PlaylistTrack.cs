using System;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class PlaylistTrack<T>
    {
        private DateTime? _addedAt;
        private T _track;

        [JsonProperty("added_at")]
        public DateTime? AddedAt
        {
            get => _addedAt;
            set
            {
                if (Track != null && value != null)
                {
                    switch (Track)
                    {
                        case FullTrack track:
                            track.AddedAt = (DateTime)AddedAt;
                            break;
                        case FullEpisode episode:
                            break;
                    }
                }
                _addedAt = value;
            }
        }
        public PublicUser AddedBy { get; set; } = default!;
        public bool IsLocal { get; set; }

        [JsonConverter(typeof(PlayableItemConverter))]
        public T Track
        {
            get => _track;
            set
            {
                if (AddedAt != null && value != null)
                {
                    switch (value)
                    {
                        case FullTrack track:
                            track.AddedAt = (DateTime)AddedAt;
                            break;
                        case FullEpisode episode:
                            break;
                    }
                }
                _track = value;
            }
        }
    }
}