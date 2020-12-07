using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using SpotifyLib.Ids;
using SpotifyProto;

namespace SpotifyLib.Models
{
    public class TrackOrEpisode
    {
        public readonly IPlayableId id;
        public readonly Track track;
        public readonly Episode episode;

        public TrackOrEpisode([CanBeNull] Track track, [CanBeNull] Episode episode)
        {
            if (track == null && episode == null) throw new ArgumentOutOfRangeException();

            this.track = track;
            this.episode = episode;

            if (track != null) id = PlayableId.From(track);
            else id = PlayableId.From(episode);
        }

        public int Duration() => track?.Duration ?? episode.Duration;
    }
}
