using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Ids;
using SpotifyLib.Models;
using SpotifyLib.SpotifyConnect.Models;

namespace SpotifyLib.SpotifyConnect.Listeners
{
    public interface IPlayerQueueEntryListener
    {
        /// <summary>
        /// An error occurred during playback.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ex"></param>
        void PlaybackError([NotNull] PlayerQueueEntry entry, [NotNull] Exception ex);

        /// <summary>
        /// The playback of the current entry ended.
        /// </summary>
        /// <param name="entry"></param>
        Task PlaybackEnded([NotNull] PlayerQueueEntry entry);


        /// <summary>
        /// The playback halted while trying to receive a chunk.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="chunk"></param>
        void PlaybackHalted([NotNull] PlayerQueueEntry entry, int chunk);


        /// <summary>
        /// The playback resumed from halt.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="chunk"></param>
        /// <param name="diff"></param>
        void PlaybackResumed([NotNull] PlayerQueueEntry entry, int chunk, int diff);


        /// <summary>
        /// Notify that a previously request instant has been reached. This is called from the runner, be careful.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="callbackId"></param>
        /// <param name="exactTime"></param>
        void InstantReached([NotNull] PlayerQueueEntry entry, int callbackId, int exactTime);


        /// <summary>
        /// The track started loading.
        /// </summary>
        /// <param name="entry"></param>
        void StartedLoading([NotNull] PlayerQueueEntry entry);


        /// <summary>
        /// The track failed loading.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ex"></param>
        /// <param name="retried"></param>
        void LoadingError([NotNull] PlayerQueueEntry entry, [NotNull] Exception ex, bool retried);


        /// <summary>
        /// The track finished loading.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="metadata"></param>
        void FinishedLoading([NotNull] PlayerQueueEntry entry, [NotNull] TrackOrEpisode metadata);


        /// <summary>
        /// Get the metadata for this content.
        /// </summary>
        /// <param name="playable"></param>
        /// <returns></returns>
        [NotNull]
        Dictionary<string, string> MetadataFor([NotNull] IPlayableId playable);
    }
}