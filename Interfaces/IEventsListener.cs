using JetBrains.Annotations;
using SpotifyLib.Ids;
using SpotifyLib.Models;

namespace SpotifyLib.Interfaces
{
    public interface IEventsListener
    {
        void OnContextChanged([NotNull] string newUri);

        void OnTrackChanged([NotNull] IPlayableId id,
            [CanBeNull] TrackOrEpisode metadata);

        void OnPlaybackPaused(long trackTime);

        void OnPlaybackResumed(long trackTime);

        void OnTrackSeeked(long trackTime);

        void OnMetadataAvailable([NotNull] TrackOrEpisode metadata);

        void OnPlaybackHaltStateChanged(bool halted, long trackTime);

        void OnInactiveSession(bool timeout);

        void OnVolumeChanged(float volume);

        void OnPanicState();
    }
}