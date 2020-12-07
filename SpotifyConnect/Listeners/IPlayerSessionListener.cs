using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Ids;
using SpotifyLib.Interfaces;
using SpotifyLib.Models;

namespace SpotifyLib.SpotifyConnect.Listeners
{
    public interface IPlayerSessionListener
    {
        [NotNull]
        IPlayableId CurrentPlayable();

        [CanBeNull]
        IPlayableId NextPlayable();

        [CanBeNull]
        IPlayableId NextPlayableDoNotSet();


        [NotNull]
        Dictionary<string, string> MetadataFor([NotNull] ISpotifyId playable);


        Task PlaybackHalted();


        Task PlaybackResumedFromHalt(long diff);

        Task StartedLoading();


        void LoadingError([NotNull] Exception ex);


        Task FinishedLoading([NotNull] TrackOrEpisode metadata);


        void PlaybackError([NotNull] Exception ex);


        Task TrackChanged([NotNull] string playbackId,
            [NotNull] TrackOrEpisode metadata,
            int pos,
            [NotNull] Reason startedReason);


        void TrackPlayed([NotNull] string playbackId,
            Reason endReason, int endedAt);
    }
}