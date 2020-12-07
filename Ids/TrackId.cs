using System;
using System.Linq;
using Base62;
using SpotifyLib.Enums;

namespace SpotifyLib.Ids
{
    public class TrackId : IPlayableId
    {
        private readonly string _locale;
        public TrackId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = SpotifyType.Track;
            var regexMatch = uri.Split(':').Last();
            this.Id = regexMatch;
            this.Uri = uri;
        }

        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }

        public string ToMercuryUri() => $"hm://metadata/4/track/{ToHexId()}?locale={_locale}";

        public SpotifyType Type { get; }

        public override bool Equals(object obj) => obj is TrackId trackId && trackId?.Uri == Uri;

        protected bool Equals(TrackId other)
        {
            return Uri == other.Uri
                   && Id == other.Id
                   && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }
    }
}

