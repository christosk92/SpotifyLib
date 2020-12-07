using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Ids;
using SpotifyLib.Models;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.SpotifyConnect.Listeners;
using SpotifyLib.SpotifyConnect.Models;
using SpotifyLib.SpotifyConnect.Spotify;

namespace SpotifyLib.SpotifyConnect
{
    public class SpotifyPlayerSession : IPlayerQueueEntryListener, IDisposable
    {
        private readonly SpotifySession session;
        private readonly ISpotifyDevice sink;
        private readonly String sessionId;
        private readonly IPlayerSessionListener listener;
        private readonly PlayerQueue queue;
        private int lastPlayPos = 0;
        private Reason? lastPlayReason = null;
        private volatile bool closed = false;

        public SpotifyPlayerSession([NotNull] SpotifySession session,
            [NotNull] ISpotifyDevice sink,
            [NotNull] String sessionId,
            [NotNull] IPlayerSessionListener listener)
        {
            this.session = session;
            this.sink = sink;
            this.sessionId = sessionId;
            this.listener = listener;
            this.queue = new PlayerQueue();
            Debug.WriteLine($"Created new session. id: {sessionId}");

            //todo: clear sink

        }

        public async Task<double> CurrentTime()
        {
            if (queue.Head == null) return -1;
            else return await queue.Head.GetTime();
        }

        public async Task SeekCurrent(int pos)
        {
            if (queue.Head == null) return;

            PlayerQueueEntry entry;
            if ((entry = queue.Prev()) != null) queue.Remove(entry);
            if ((entry = queue.Next()) != null) queue.Remove(entry);

            await queue.Head.Seek(pos);
        }
        public TrackOrEpisode CurrentMetadata() => queue.Head?.Metadata;

        /// <summary>
        /// start playing this content by any possible mean. Also sets up crossfade for the previous entry and the current head.
        /// </summary>
        /// <param name="playable">The content to be played</param>
        /// <param name="pos">The time in milliseconds</param>
        /// <param name="reason">The reason why the playback started</param>
        /// <returns>The playback ID associated with the head</returns>
        public async Task<string> Play([NotNull] IPlayableId playable, 
            int pos, 
            [NotNull] Reason reason)
        {
            var j = (await PlayInternal(playable, pos, reason));
            return j.item.PlaybackId;
        }

        /// <summary>
        /// Creates and adds a new entry to the queue.
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="preloaded"></param>
        private void Add([NotNull] IPlayableId playable, 
            bool preloaded,
            int initialSeek)
        {
            var entry = new PlayerQueueEntry(sink, playable, preloaded, this, initialSeek);
            queue.Add(entry);
            if (queue.Next() == entry)
            {
                var head = queue.Head;
            }
        }

        /// <summary>
        /// Adds the next content to the queue (considered as preloading).
        /// </summary>
        private void AddNext()
        {
            var playable = listener.NextPlayableDoNotSet();
            if (playable != null) Add(playable, true, 0);
        }

        /// <summary>
        /// Tries to advance to the given content. This is a destructive operation as it will close every entry that passes by.
        /// Also checks if the next entry has the same content, in that case it advances (repeating track fix).
        /// </summary>
        /// <returns>Whether the operation completed</returns>
        private bool AdvanceTo([NotNull] IPlayableId id)
        {
            do
            {
                var entry = queue.Head;
                if (entry == null) return false;
                if (!entry.Playable.Equals(id)) continue;
                var next = queue.Next();
                if (next == null || !next.Playable.Equals(id))
                    return true;
            } while (queue.Advance());
            return false;
        }
        /// <summary>
        /// Gets the next content and tries to advance, notifying if successful.
        /// </summary>
        /// <param name="reason"></param>
        private async Task Advance([NotNull] Reason reason)
        {
            if (closed) return;

            var next = listener.NextPlayable();
            if (next == null)
                return;

            var entry = await PlayInternal(next,0, reason);
            await listener.TrackChanged(entry.item.PlaybackId, entry.item.Metadata, entry.entry, reason);
        }

        private async Task<(int entry, PlayerQueueEntry item)> PlayInternal(
            [NotNull] IPlayableId playable,
            int pos,
            [NotNull] Reason reason)
        {
            SpotifyPlayer.Current.State.SpotifyDevice.OnCurrentlyPlayingChanged(new PlayingChangedRequest
            {
                IsPaused = false,
                ItemUri = playable.Uri,
                IsPlaying = true,
                ContextUri = SpotifyPlayer.Current.State.ConnectState.ContextUri
            });
            lastPlayPos = pos;
            lastPlayReason = reason;

            if (!AdvanceTo(playable))
            {
                Add(playable, false, pos);
                queue.Advance();
            }

            var head = queue.Head;
            if (head == null)
                throw new Exception("Illegal State");

            var customFade = false;
            if (head.Prev != null)
            {
                head.Prev?.Dispose();
                customFade = false;
                //TODO: Crossfade
            }

            var @out = sink;
            if (@out == null)
                throw new Exception("No output is available for " + head);
            await head.Seek(pos);
            Debug.WriteLine("{0} has been added to the output. sessionId: {0}, pos: {1}, reason: {2}", head, sessionId, pos,
                reason);
            return (pos, queue.Head);
        }


        public void PlaybackError(PlayerQueueEntry entry, Exception ex)
        {
            throw new NotImplementedException();
        }

        public async Task PlaybackEnded(PlayerQueueEntry entry)
        {
            listener.TrackPlayed(entry.PlaybackId, 
                entry.EndReason, 0);

            if (entry == queue.Head)
                await Advance(Reason.trackdone);
        }

        public void PlaybackHalted(PlayerQueueEntry entry,int index)
        {
            if (entry == queue.Head) listener.PlaybackHalted();
        }

        public void PlaybackResumed(PlayerQueueEntry entry, int index, int diff)
        {
            if (entry == queue.Head) listener.PlaybackResumedFromHalt(diff);
        }

        public void InstantReached(PlayerQueueEntry entry, int callbackId, int exactTime)
        {
            throw new NotImplementedException();
        }

        public void StartedLoading(PlayerQueueEntry entry)
        {
            Debug.WriteLine("{0} started loading.", entry);
            if (entry == queue.Head) listener.StartedLoading();
        }

        public void LoadingError(PlayerQueueEntry entry, Exception ex, bool retried)
        {
            throw new NotImplementedException();
        }

        public void FinishedLoading(PlayerQueueEntry entry, TrackOrEpisode metadata)
        {
            Debug.WriteLine("{0} finished loading.", entry);
            if (entry == queue.Head) listener.FinishedLoading(metadata);
        }

        public Dictionary<string, string> MetadataFor(IPlayableId playable)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            sink?.Dispose();
        }
    }
}
