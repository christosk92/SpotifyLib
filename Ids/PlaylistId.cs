using System;
using System.Text.RegularExpressions;
using Base62;
using SpotifyLib.Enums;
using SpotifyLib.Interfaces;

namespace SpotifyLib.Ids
{
    public class PlaylistId : ISpotifyId
    {
        public PlaylistId(string uri, bool chart = false)
        {
            Type = SpotifyType.Playlist;
            var regexMatch = Regex.Match(uri, "spotify:user:(.*):playlist:(.{22})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[2].Value;
                this.Username = regexMatch.Groups[1].Value;
                PlaylistType = PlaylistType.UserPlaylist;
            }
            else
            {
                regexMatch = Regex.Match(uri, "spotify:playlist:(.{22})");
                if (regexMatch.Success)
                {
                    this.Id = regexMatch.Groups[1].Value;
                    this.Username = regexMatch.Groups[1].Value;
                    PlaylistType = PlaylistType.UserPlaylist;
                }
                else
                {
                    regexMatch = Regex.Match(uri, "spotify:dailymix:(.{22})");
                    if (regexMatch.Success)
                    {
                        this.Id = regexMatch.Groups[1].Value;
                        PlaylistType = PlaylistType.DailyMixList;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(uri), "Not a Spotify album ID: " + uri);
                    }
                }
            }
            if (chart)
                PlaylistType = PlaylistType.DailyMixList;

            this.Uri = uri;
        }


        public string Uri { get; }
        public string Id { get; }
        public string Username { get; }

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

        public SpotifyType Type { get; }
        public PlaylistType PlaylistType { get; }

        public string ToMercuryUri() => ToMercuryUri(false);

        public string ToMercuryUri(bool annotate)
        {
            //return $"hm://playlist/{string.Join("/", Uri.Split(':').Skip(1))}";
            if (annotate)
                return $"hm://playlist-annotate/v1/annotation/user/{Username}/playlist/{Id}?format=json";
            return $"hm://playlist/v2/playlist/{Id}?format=json&locale=ja&from=0&length=100";
        }

        protected bool Equals(PlaylistId other)
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