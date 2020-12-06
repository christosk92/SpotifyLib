using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Interfaces;

namespace SpotifyLib.Ids
{
    public class ArtistId : ISpotifyId
    {
        private readonly string _locale;

        public static ArtistId FromHex(string hex)
        {
            var k = (Utils.hexToBytes(hex)).ToBase62(true);
            var j = "spotify:artist:" + k;
            return new ArtistId(j);
        }

        public ArtistId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = SpotifyType.Artist;
            this.Id = uri.Split(':').Last();
            this.Uri = uri;
        }

        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            //Utils.bytesToHex(BASE62.decode(id.getBytes(), 16))
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }

        public string ToMercuryUri() => $"hm://artist/v1/{Id}/desktop?format=json&catalogue=premium&locale={_locale}&cat=1";

        public string ToMercuryUriDetailed() =>
            $"hm://artist-identity-view/v2/profile/{Id}?fields=name,autobiography,biography,gallery,monthlyListeners,avatar&imgSize=large";

        public string MercuryInsights() =>
            $"hm://creatorabout/v0/artist-insights/{Id}";

        public SpotifyType Type { get; }

        protected bool Equals(ArtistId other)
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
