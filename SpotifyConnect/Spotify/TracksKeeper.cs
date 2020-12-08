using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Connectstate;
using JetBrains.Annotations;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLib.Helpers;
using SpotifyLib.Ids;
using SpotifyLib.SpotifyConnect.Helpers;

namespace SpotifyLib.SpotifyConnect.Spotify
{
    public enum NextPlayable
    {
        MissingTracks,
        Autoplay,
        OkPlay,
        OkPause,
        OkRepeat
    }

    public enum PreviousPlayable
    {
        MISSING_TRACKS,
        OK
    }

    public static class NextPlayableHelpers
    {
        public static bool IsOk(this NextPlayable enumVal)
        {
            return enumVal == NextPlayable.OkPlay || enumVal == NextPlayable.OkPause || enumVal == NextPlayable.OkRepeat;
        }
        public static bool IsOk(this PreviousPlayable enumVal)
        {
            return enumVal == PreviousPlayable.OK;
        }
    }

    public class FisherYatesShuffle<T>
    {
        //TODO
        private readonly Random random;
        private volatile int currentSeed;
        private volatile int sizeForSeed = -1;

        private static int[] GetShuffleExchanges(int size, int seed)
        {
            int[] exchanges = new int[size - 1];
            var rand = new Random(seed);
            for (int i = size - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                exchanges[size - 1 - i] = n;
            }

            return exchanges;
        }

        public void Shuffle([NotNull] List<T> list, bool saveSeed)
            => Shuffle(list, 0, list.Count, saveSeed);

        public void Shuffle([NotNull] List<T> list, int from, int to, bool saveSeed)
        {
            var seed = random.Next();
            if (saveSeed) currentSeed = seed;

            var size = to - from;
            if (saveSeed) sizeForSeed = size;

            var exchanges = GetShuffleExchanges(size, seed);
            for (var i = size - 1; i > 0; i--)
            {
                var n = exchanges[size - 1 - i];
                list.Swap(from + n, from + i);
            }
        }

        public void Unshuffle([NotNull] List<T> list) => Unshuffle(list, 0, list.Count);

        public void Unshuffle([NotNull] List<T> list, int from, int to)
        {
            if (currentSeed == 0) throw new Exception("Current seed is zero!");
            if (sizeForSeed != to - from) throw new Exception("Size mismatch! Cannot unshuffle.");

            var size = to - from;
            var exchanges = GetShuffleExchanges(size, currentSeed);
            for (var i = 1; i < size; i++)
            {
                var n = exchanges[size - i - 1];
                list.Swap(from + n, from + i);
            }

            currentSeed = 0;
            sizeForSeed = -1;
        }

        public bool CanUnshuffle(int size) => currentSeed != 0 && sizeForSeed == size;
    }

    internal class TracksKeeper
    {
        private static readonly int MaxPrevTracks = 16;
        private static readonly int MaxNextTracks = 48;

        private readonly LinkedList<ContextTrack> _queue = new LinkedList<ContextTrack>();
        public readonly List<ContextTrack> Tracks = new List<ContextTrack>();
        private readonly FisherYatesShuffle<ContextTrack> _shuffle = new FisherYatesShuffle<ContextTrack>();

        private volatile bool _isPlayingQueue = false;
        private volatile bool _cannotLoadMore = false;
        private volatile int _shuffleKeepIndex = -1;

        private readonly SpotifyState _state;

        internal TracksKeeper(SpotifyState state)
        {
            _state = state;
            CheckComplete();
        }
        public void InitializeFrom(
            [NotNull] Func<List<ContextTrack>, int> finder, 
            [CanBeNull] ContextTrack track, 
            [CanBeNull] Queue contextQueue) 
        {
            Tracks.Clear();
            _queue.Clear();

            while (true) {
                if (_state.Pages.NextPage()) {
                    var newTracks = _state.Pages.CurrentPage();
                    var index = finder(newTracks);
                    if (index == -1) {
                        Tracks.AddRange(newTracks);
                        continue;
                    }

                    index += Tracks.Count();
                    Tracks.AddRange(newTracks);

                    SetCurrentTrackIndex(index);
                    break;
                } else
                {
                    _cannotLoadMore = true;
                    UpdateTrackCount();
                    throw new Exception("Couldn't find current track!");
                }
            }

            if (contextQueue != null)
            {
                foreach (var t in contextQueue.Tracks)
                {
                    _queue.Add(t);
                }
                _isPlayingQueue = contextQueue.IsPlayingQueue;
                UpdateState();
            }

            CheckComplete();
            if (!PlayableId.CanPlaySomething(Tracks))
                throw new Exception("Cannot play anything");

            if (track != null) EnrichCurrentTrack(track);
        }
        public void SkipTo([NotNull] ContextTrack track)
        {
            SkipTo(track.Uri);
            EnrichCurrentTrack(track);
        }
        public PreviousPlayable PreviousPlayable()
        {
            var index = GetCurrentTrackIndex();
            if (_isPlayingQueue)
            {
                index += 1;
                _isPlayingQueue = false;
            }

            if (index == 0)
            {
                if (_state.IsRepeatingContext() && _state.Context.IsFinite())
                    SetCurrentTrackIndex(Tracks.Count - 1);
            }
            else
            {
                SetCurrentTrackIndex((int)index - 1);
            }

            if (!ShouldPlay(Tracks[(int)GetCurrentTrackIndex()]))
                return PreviousPlayable();

            return Spotify.PreviousPlayable.OK;
        }
      

        public void SkipTo([NotNull] string uri)
        {
            if (_queue.Any())
            {
                var queueCopy = new List<ContextTrack>();

                foreach (var queue in _queue)
                {
                    if (queue.Uri.Equals(uri))
                    {
                        _isPlayingQueue = true;
                        UpdateState();
                        return;
                    }

                    _queue.Remove(queue);
                }

                _queue.Clear();
                _queue.Add(queueCopy);
            }

            var index = Tracks.FindIndex(x => x.Uri == uri);
            if (index == -1) throw new Exception("Illegal State");

            SetCurrentTrackIndex(index);
        }
        public void AddToQueue([NotNull] ContextTrack track)
        {
            track.Metadata["is_queued"] = "true";
            _queue.Add(track);
            UpdatePrevNextTracks();
            UpdateTrackCount();
        }
        public void EnrichCurrentTrack([NotNull] ContextTrack track)
        {
            if (_isPlayingQueue)
            {
                var b = _state.ConnectState.Track;
                ProtoUtils.EnrichTrack(b, track);
            }
            else
            {
                int index = (int) GetCurrentTrackIndex();
                var current = Tracks[index];
                ProtoUtils.EnrichTrack(current, track);
                Tracks[index] = current;
                _state.ConnectState.Track = ProtoUtils.ConvertToProvidedTrack(current);
            }
        }

        public void UpdateContext([NotNull] List<ContextPage> updatedPages)
        {
            var updatedTracks = updatedPages
                .SelectMany(x => x.Tracks).ToList();
            foreach (var track in updatedTracks)
            {
                var index = Tracks.FindIndex(x => x.Uri == track.Uri);
                if (index == -1) continue;

                var b = new ContextTrack(Tracks[index]);
                ProtoUtils.CopyOverMetadata(track, b);
                Tracks[index] = b;

                if (index != GetCurrentTrackIndex()) continue;
                ProtoUtils.CopyOverMetadata(track, _state.ConnectState.Track);
                UpdateLikeDislike();
            }
        }

        public void ToggleShuffle(bool value)
        {
            if (!_state.Context.IsFinite()) throw new Exception("Cannot shuffle infinite context!");
            if (Tracks.Count() <= 1) return;
            if (_isPlayingQueue) return;

            if (value)
            {
                if (!_cannotLoadMore)
                {
                    if (LoadAllTracks())
                    {
                        Debug.WriteLine("Loaded all tracks before shuffling.");
                    }
                    else
                    {
                        Debug.WriteLine("Cannot shuffle context!");
                        return;
                    }
                }

                var currentlyPlaying = _state.GetCurrentPlayableOrThrow();
                _shuffle.Shuffle(Tracks, true);
                _shuffleKeepIndex = Tracks.FindIndex(x => x.Uri == currentlyPlaying.Uri);
                Tracks.Swap(0, _shuffleKeepIndex);
                SetCurrentTrackIndex(0);

                Debug.WriteLine("Shuffled context! keepIndex: {0}", _shuffleKeepIndex);
            }
            else
            {
                if (_shuffle.CanUnshuffle(Tracks.Count))
                {
                    var currentlyPlaying = _state.GetCurrentPlayableOrThrow();
                    if (_shuffleKeepIndex != -1) Tracks.Swap(0, _shuffleKeepIndex);

                    _shuffle.Unshuffle(Tracks);
                    SetCurrentTrackIndex(Tracks.FindIndex(x => x.Uri == currentlyPlaying.Uri));

                    Debug.WriteLine("Unshuffled using Fisher-Yates.");
                }
                else
                {
                    var id = _state.GetCurrentPlayableOrThrow();

                    Tracks.Clear();
                    _state.Pages = PagesLoader.From(_state.Session, _state.Context.Uri());
                    LoadAllTracks();

                    SetCurrentTrackIndex(Tracks.FindIndex(x => x.Uri == id.Uri));
                    Debug.WriteLine("Unshuffled by reloading context.");
                }
            }
        }

        public (int index, IPlayableId id)? NextPlayableDoNotSet()
        {
            if (_state.ConnectState.Options.RepeatingTrack)
                return ((int) GetCurrentTrackIndex(), PlayableId.From(Tracks[(int) GetCurrentTrackIndex()]));

            if (_queue.Any())
                return (-1, PlayableId.From(_queue.First()));

            int current = (int) GetCurrentTrackIndex();
            if (current == Tracks.Count - 1)
            {
                if (_state.IsShufflingContext() || _cannotLoadMore) return null;

                if (_state.Pages.NextPage())
                {
                    Tracks.AddRange(_state.Pages.CurrentPage());
                }
                else
                {
                    _cannotLoadMore = true;
                    UpdateTrackCount();
                    return null;
                }
            }

            if (!_state.Context.IsFinite() && Tracks.Count - current <= 5)
            {
                if (_state.Pages.NextPage())
                {
                    Tracks.AddRange(_state.Pages.CurrentPage());
                    Debug.WriteLine("Preloaded next page due to infinite context.");
                }
                else
                {
                    Debug.WriteLine("Couldn't (pre)load next page of context!");
                }
            }

            int add = 1;
            while (true)
            {
                var track = Tracks[current + add];
                if (ShouldPlay(track)) break;
                add++;
            }

            return (current + add, PlayableId.From(Tracks[(current + add)]));
        }

        private void CheckComplete()
        {
            if (_cannotLoadMore) return;

            if (_state.Context.IsFinite())
            {
                var totalTracks = int.Parse(_state.ConnectState.ContextMetadata.FirstOrDefault(x =>
                    x.Key == "track_count").Value ?? "-1");
                if (totalTracks == -1) _cannotLoadMore = false;
                else _cannotLoadMore = totalTracks == Tracks.Count;
            }
            else
            {
                _cannotLoadMore = false;
            }
        }

        private void UpdateLikeDislike()
        {
            var a = _state.ConnectState.ContextMetadata.ContainsKey("like-feedback-selected");
            var a2 = "0";
            if(a)
                a2 = _state.ConnectState.ContextMetadata["like-feedback-selected"];
            if (a2 == "1")
            {
                _state.ConnectState.ContextMetadata["like-feedback-selected"] =
                    _state.ConnectState.Track.Metadata.ContainsKey("like-feedback-selected") 
                        ? _state.ConnectState.Track.Metadata["like-feedback-selected"] : "0";
            }
            else
            {
                _state.ConnectState.ContextMetadata.Remove("like-feedback-selected");
            }

            var b = _state.ConnectState.ContextMetadata.ContainsKey("dislike-feedback-enabled");
            var b2 = "0";
            if (b)
                b2 = _state.ConnectState.ContextMetadata["dislike-feedback-enabled"];
            if (b2 == "1")
            {
                _state.ConnectState.ContextMetadata["dislike-feedback-selected"] =
                    _state.ConnectState.Track.Metadata.ContainsKey("dislike-feedback-selected")
                        ? _state.ConnectState.Track.Metadata["dislike-feedback-selected"] : "0";
            }
            else
            {
                _state.ConnectState.ContextMetadata.Remove("dislike-feedback-selected");
            }
        }

        private void UpdateTrackDuration()
        {
            var current = _state.ConnectState.Track;
            _state.ConnectState.Duration = current.Metadata.ContainsKey("duration")
                ? long.Parse(current.Metadata["duration"])
                : 0L;
        }

        public void UpdateTrackDuration(int duration)
        {
            _state.ConnectState.Duration = duration;
            _state.ConnectState.Track.Metadata["duration"] = duration.ToString();
            UpdateMetadataFor((int) GetCurrentTrackIndex(), "duration", duration.ToString());
        }

        public void UpdateState()
        {
            if (_isPlayingQueue)
            {
                _state.ConnectState.Track = ProtoUtils.ConvertToProvidedTrack(_queue.First.Value);
                _queue.RemoveFirst();
            }
            else _state.ConnectState.Track = ProtoUtils.ConvertToProvidedTrack(Tracks[(int) GetCurrentTrackIndex()]);

            UpdateLikeDislike();

            UpdateTrackDuration();
            UpdatePrevNextTracks();
        }

        private void UpdatePrevNextTracks()
        {
            var index = GetCurrentTrackIndex();

            _state.ConnectState.PrevTracks.Clear();
            for (var i = (int) Math.Max(0, index - MaxPrevTracks); i < index; i++)
                _state.ConnectState.PrevTracks.Add(ProtoUtils.ConvertToProvidedTrack(Tracks[i]));

            _state.ConnectState.NextTracks.Clear();
            _state.ConnectState.NextTracks.AddRange(_queue.Select(ProtoUtils.ConvertToProvidedTrack));


            for (var i = (int) index + 1; i < Math.Min(Tracks.Count, index + 1 + MaxNextTracks); i++)
                _state.ConnectState.NextTracks.Add((ProtoUtils.ConvertToProvidedTrack(Tracks[i])));
        }

        public void UpdateMetadataFor(int index, [NotNull] string key, [NotNull] string value)
        {
            var ct = Tracks[index];
            ct.Metadata[key] = value;
            Tracks[index] = ct;
        }

        public void UpdateMetadataFor([NotNull] string uri, [NotNull] string key, [NotNull] string value)
        {
            int index = Tracks.FindIndex(x => x.Uri == uri);
            if (index == -1) return;

            UpdateMetadataFor(index, key, value);
        }

        public void SetRepeatingTrack(bool value)
        {
            if (_state.Context == null) return;
            _state.ConnectState.Options.RepeatingTrack =
                value && _state.Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_TRACK);
        }

        public bool ShouldPlay([NotNull] ContextTrack track)
        {
            if (!PlayableId.IsSupported(track.Uri) || !PlayableId.ShouldPlay(track))
                return false;

            var filterExplicit = _state.Session.UserAttributes.ContainsKey("filter-explicit-content");
            filterExplicit = !filterExplicit 
                             || bool.Parse(_state.Session.UserAttributes["filter-explicit-content"]);

            if (!filterExplicit) return true;

            return !bool.Parse(track.Metadata.ContainsKey("is_explicit") ? track.Metadata["is_explicit"] : "false");
        }

        public NextPlayable NextPlayable(bool autoplayEnabled)
        {
            if (_state.ConnectState.Options.RepeatingTrack)
            {
                SetRepeatingTrack(false);
                return Spotify.NextPlayable.OkRepeat;
            }

            if (_queue.Any())
            {
                _isPlayingQueue = true;
                UpdateState();

                if (!ShouldPlay(Tracks[(int) GetCurrentTrackIndex()]))
                    return NextPlayable(autoplayEnabled);

                return Spotify.NextPlayable.OkPlay;
            }

            _isPlayingQueue = false;

            var play = true;
            var next = NextPlayableDoNotSet();
            if (next == null || next.Value.index == -1)
            {
                if (!_state.Context.IsFinite()) return Spotify.NextPlayable.MissingTracks;

                if (_state.IsRepeatingContext())
                {
                    SetCurrentTrackIndex(0);
                }
                else
                {
                    if (autoplayEnabled)
                    {
                        return Spotify.NextPlayable.Autoplay;
                    }
                    else
                    {
                        SetCurrentTrackIndex(0);
                        play = false;
                    }
                }
            }
            else
            {
                SetCurrentTrackIndex(next.Value.index);
            }

            return play ? Spotify.NextPlayable.OkPlay : Spotify.NextPlayable.OkPause;
        }

        private void SetCurrentTrackIndex(int index)
        {
            if (_isPlayingQueue) throw new Exception("Illegal State");
            _state.ConnectState.Index = new ContextIndex
            {
                Track = (uint) index
            };
            UpdateState();
        }

        public bool IsPlayingFirst()
        {
            return GetCurrentTrackIndex() == 0;
        }

        public bool IsPlayingLast()
        {
            if (_cannotLoadMore && !_queue.Any()) return GetCurrentTrackIndex() == Tracks.Count;
            return false;
        }

        private uint GetCurrentTrackIndex() => _state.ConnectState.Index.Track;


        public void InitializeStart()
        {
            if (!_state.Pages.NextPage()) throw new Exception("Illegal State");

            Tracks.Clear();
            Tracks.AddRange(_state.Pages.CurrentPage());

            CheckComplete();
            if (!PlayableId.CanPlaySomething(Tracks))
                throw new Exception("UnsupportedContextException");

            var transformingShuffle = _state.ConnectState.ContextMetadata.ContainsKey("transform.shuffle");
            transformingShuffle = transformingShuffle && bool.Parse(_state.ConnectState.ContextMetadata["transform.shuffle"]);
            if (_state.Context.IsFinite() && _state.IsShufflingContext() && transformingShuffle) ShuffleEntirely();
            else _state.ConnectState.Options.ShufflingContext = false; // Must do this directly!

            SetCurrentTrackIndex(0);
        }

        public void ShuffleEntirely()
        {
            if (!_state.Context.IsFinite()) throw new Exception("Cannot shuffle infinite context!");
            if (Tracks.Count <= 1) return;
            if (_isPlayingQueue) return;

            if (!_cannotLoadMore)
            {
                if (LoadAllTracks())
                {
                    Debug.WriteLine("Loaded all tracks before shuffling (entirely).");
                }
                else
                {
                    Debug.WriteLine("Cannot shuffle entire context!");
                    return;
                }
            }

            _shuffle.Shuffle(Tracks, true);
            Debug.WriteLine("Shuffled context entirely!");
        }

        public bool LoadAllTracks()
        {
            if (!_state.Context.IsFinite()) throw new Exception("Illegal State");

            try
            {
                while (true)
                {
                    if (_state.Pages.NextPage()) Tracks.AddRange(_state.Pages.CurrentPage());
                    else break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed loading all tracks!", ex);
                return false;
            }

            _cannotLoadMore = true;
            UpdateTrackCount();
            return true;
        }

        private void UpdateTrackCount()
        {
            if (_state.Context.IsFinite())
                _state.ConnectState.ContextMetadata["track_count"] = (Tracks.Count + _queue.Count).ToString();
            else
                _state.ConnectState.ContextMetadata.Remove("track_count");
        }

    }
}