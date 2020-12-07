using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Ids;
using SpotifyLib.Interfaces;
using SpotifyLib.Models;
using SpotifyLib.SpotifyConnect.Listeners;

namespace SpotifyLib.SpotifyConnect.Models
{
    public class PlayerQueueEntry : Entry,
        IHaltListener,
        IDisposable
    {
        public volatile bool Closed = false;
        public readonly IPlayableId Playable;
        public TrackOrEpisode Metadata;
        public readonly string PlaybackId;
        private readonly bool preloaded;
        private readonly IPlayerQueueEntryListener listener;
        private readonly ManualResetEvent playbackLock = new ManualResetEvent(false);
        private readonly ISpotifyDevice sink;
        private readonly Dictionary<int, int> notifyInstants = new Dictionary<int, int>();
        private long playbackHaltedAt = 0;
        private const bool Retried = false;
        public Reason EndReason = Reason.trackdone;
        private volatile int seekTime = -1;
        private static IPlayableId _current;

        public PlayerQueueEntry(
            [NotNull] ISpotifyDevice sink,
            [NotNull] IPlayableId playable,
            bool preloaded,
            [NotNull] IPlayerQueueEntryListener listener,
            int initialSeek)
        {
            this.sink = sink;
            Playable = playable;
            this.preloaded = preloaded;
            this.listener = listener;
            previousCommand = null;
            this.PlaybackId = SpotifyState.GeneratePlaybackId();
            _current = playable;
            Debug.WriteLine($"Created new {this}");

            this.sink.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            this.sink.MediaOpened += MediaPlayer_MediaOpened;
            this.sink.MediaEnded += MediaPlayer_MediaEnded;
            listener.StartedLoading(this);
            _ = Load(preloaded, initialSeek);
        }

        private void MediaPlayer_MediaEnded(object sender, object args)
        {
            listener.PlaybackEnded(this);
            sink.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            sink.MediaOpened -= MediaPlayer_MediaOpened;
            sink.MediaEnded -= MediaPlayer_MediaEnded;
        }

        private async void MediaPlayer_MediaOpened(object sender, object args)
        {
        }

        private MediaPlaybackState? previousCommand;
        private async void PlaybackSession_PlaybackStateChanged(object s, object args)
        {
            if (!Playable.Uri.Equals(_current?.Uri))
            {
                Dispose();
            }

            var sender = s as ISpotifyDevice;
            switch (sender.PlaybackState)
            {
                case MediaPlaybackState.None:
                    break;
                case MediaPlaybackState.Opening:
                    Debug.WriteLine("MEDIA PLAYER Opening");
                    break;
                case MediaPlaybackState.Buffering:
                    Debug.WriteLine("MEDIA PLAYER Buffering");
                    break;
                case MediaPlaybackState.Playing:
                    //  Debug.WriteLine("MEDIA PLAYER PLAYING");
                    if (playbackHaltedAt == 0) return;
                    var duration = (int) (sender.Position.TotalMilliseconds);
                    listener.PlaybackResumed(this, -1, duration);
                    previousCommand = MediaPlaybackState.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    //Debug.WriteLine("MEDIA PLAYER PAUSED");
                    if (previousCommand == MediaPlaybackState.Paused) return;
                    var duration2 = (int) (sender.Position.TotalMilliseconds);
                    playbackHaltedAt = duration2;
                    listener.PlaybackHalted(this, -1);
                    previousCommand = MediaPlaybackState.Paused;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task Load(bool preloaded, int initialSeek)
        {
            try
            {
                var j = await sink.Load(Playable, preloaded, this, initialSeek);
                Metadata = new TrackOrEpisode(j.track, j.episode);

                switch (Playable.Type)
                {
                    case SpotifyType.Episode:
                        Debug.WriteLine("Loaded episode. name: '{0}', uri: {1}, id: {2}", j.episode.Name, Playable.Uri,
                            PlaybackId);
                        break;
                    case SpotifyType.Track:
                        Debug.WriteLine("Loaded track. name: '{0}', uri: {1}, id: {2}", j.track.Name, Playable.Uri,
                            PlaybackId);
                        break;
                }

                if (initialSeek != -1)
                {
                    sink.Position = TimeSpan.FromMilliseconds(initialSeek);
                    seekTime = -1;
                }
                listener.FinishedLoading(this, Metadata);
            }
            catch (Exception x)
            {
                Dispose();
                listener.LoadingError(this, x, Retried);
                Debug.WriteLine("{0} terminated at loading. " + x.ToString(), this);
                return;
            }
        }

        public void StreamReadHalted(int chunk, long time)
        {
            playbackHaltedAt = time;
            listener.PlaybackHalted(this, chunk);
        }

        public void StreamReadResumed(int chunk, long time)
        {
            if (playbackHaltedAt == 0) return;

            int duration = (int)(time - playbackHaltedAt);
            listener.PlaybackResumed(this, chunk, duration);
        }
        public async Task Seek(int pos)
        {
            seekTime = pos;
            if (seekTime != -1)
            {
                sink.Position = TimeSpan.FromMilliseconds(seekTime);
                seekTime = -1;
            }
        }

        public bool CloseIfUseless => false;

        public virtual void Dispose(bool val)
        {

        }

        public async Task<double> GetTime()
        {
            double r = 0;
            r =  sink.Position.TotalMilliseconds;
            return r;
        }

        public void Dispose()
        {
            Closed = true;
            sink.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            Dispose(true);
        }

        public override string ToString()
        {
            return "PlayerQueueEntry{" + PlaybackId + "}";
        }
    }
}