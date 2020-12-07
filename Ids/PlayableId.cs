using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connectstate;
using JetBrains.Annotations;
using Spotify.Player.Proto;
using SpotifyLib.Helpers;
using SpotifyProto;

namespace SpotifyLib.Ids
{
    public static class PlayableId
    {
        public static bool CanPlaySomething([NotNull] List<ContextTrack> tracks)
        {
            return tracks.Any(x => IsSupported(x.Uri) && ShouldPlay(x));
        }

        public static bool ShouldPlay([NotNull] ContextTrack track)
        {
            string forceRemoveReasons = null;
            if (track.Metadata.ContainsKey("force_remove_reasons"))
                forceRemoveReasons = track.Metadata["force_remove_reasons"];
            return forceRemoveReasons == null || forceRemoveReasons.IsEmpty();
        }
        public static bool IsSupported([NotNull] string uri)
        {
            return !uri.StartsWith("spotify:local:") && !object.Equals(uri, "spotify:delimiter")
                                                     && !object.Equals(uri, "spotify:meta:delimiter");
        }
        public static IPlayableId From([NotNull] ContextTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }
        public static IPlayableId From([NotNull] ProvidedTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }

        public static IPlayableId FromUri([NotNull] string uri)
        {
            if (!IsSupported(uri)) throw new Exception("Unsupported id.");

            if (uri.Split(':')[1] == "track")
            {
                return new TrackId(uri);
            }
            if (uri.Split(':')[1] == "episode")
            {
                return new EpisodeId(uri);
            }
            else
            {
                throw new Exception("Unknown uri: " + uri);
            }
        }
        public static IPlayableId From([NotNull] Track track)
            => new TrackId($"spotify:track:{Base62.EncodingExtensions.ToBase62(track.Gid.ToByteArray(), true)}");
        public static IPlayableId From([NotNull] Episode track)
            => new TrackId($"spotify:episode:{Base62.EncodingExtensions.ToBase62(track.Gid.ToByteArray(), true)}");
    }
}