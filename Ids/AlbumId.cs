using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Base62;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Interfaces;

namespace SpotifyLib.Ids
{
    public class AlbumId : ISpotifyId
    {
        private readonly string _locale;

        public static AlbumId FromHex(string hex)
        {
            var k = (Utils.hexToBytes(hex)).ToBase62(true);
            var j = "spotify:album:" + k;
            return new AlbumId(j);
        }

        public AlbumId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = SpotifyType.Album;
            var regexMatch = Regex.Match(uri, "spotify:album:(.{22})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(uri), "Not a Spotify album ID: " + uri);
            }
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

        public string ToMercuryUri() => $"hm://album/v1/album-app/album/{Uri}/desktop?country=jp&catalogue=premium&locale={_locale}";

        public SpotifyType Type { get; }

        protected bool Equals(AlbumId other)
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
