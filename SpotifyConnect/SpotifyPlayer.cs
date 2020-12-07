using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLib.Enums;
using SpotifyLib.Ids;
using SpotifyLib.Interfaces;
using SpotifyLib.Mercury;
using SpotifyLib.Models;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.SpotifyConnect.Events;
using SpotifyLib.SpotifyConnect.Handlers;
using SpotifyLib.SpotifyConnect.Helpers;
using SpotifyLib.SpotifyConnect.Listeners;
using SpotifyLib.SpotifyConnect.Models;
using SpotifyLib.SpotifyConnect.Spotify;
using SpotifyLib.SpotifyConnect.Transitions;

namespace SpotifyLib.SpotifyConnect
{
    public class SpotifyPlayer : IDeviceStateHandlerListener,
        IPlayerSessionListener,
        IAudioSinkListener
    {
        public static SpotifyPlayer Current;
        public static readonly int VolumeMax = 65536;

        private readonly SpotifySession _session;
        private readonly EventsDispatcher events;
        public SpotifyState State;
        public SpotifyPlayerSession PlayerSession;
        protected readonly EventsHandler eventshandler = new EventsHandler();
        public ISpotifyDevice Device;
        public SpotifyPlayer(SpotifySession session, 
            ISpotifyDevice device,
            uint initialVolume,
            int volumeSteps)
        {
            _session = session;
            this.events = new EventsDispatcher( this);
            Device = device;
            InitState(initialVolume, volumeSteps);
            Current = this;
        }

        public SpotifyPlayer()
        {

        }

        private bool _autoPlay;

        public bool AutoPlay
        {
            get => _autoPlay;
            set
            {
                _autoPlay = value;
            }
        }

        #region LocalPlayback

        public TrackOrEpisode CurrentMetadata() => PlayerSession?.CurrentMetadata();

        public void AddEventsListener([NotNull] IEventsListener listener)
        {
            events.Listeners.Add(listener);
        }

        public void RemoveEventsListener([NotNull] IEventsListener listener)
        {
            events.Listeners.Remove(listener);
        }

        private void InitState(uint initialVolume, int volumeSteps)
        {
            this.State = new SpotifyState(_session,
                this,
                initialVolume,
                volumeSteps);
            State.AddListener(this);
        }

        public async Task Ready()
        {
            throw new NotImplementedException();
        }

        public async Task Command(Endpoint endpoint, CommandBody data)
        {
            Debug.WriteLine("Received command: " + endpoint);

            switch (endpoint)
            {
                case Endpoint.Play:
                    await HandlePlay(data.Obj);
                    break;
                case Endpoint.Pause:
                    _ = HandlePause();
                    break;
                case Endpoint.Resume:
                    _ = HandleResume();
                    break;
                case Endpoint.SeekTo:
                    await HandleSeek(int.Parse(data.Value));
                    break;
                case Endpoint.SkipNext:
                    _ =  HandleSkipNext(data.Obj, TransitionInfo.SkippedNext(State));
                    break;
                case Endpoint.SkipPrev:
                    _ = HandleSkipPrev();
                    break;
                case Endpoint.SetShufflingContext:
                    break;
                case Endpoint.SetRepeatingContext:
                    break;
                case Endpoint.SetRepeatingTrack:
                    break;
                case Endpoint.UpdateContext:
                    var j = PlayCommandHelper.GetContext(data.Obj) as JObject;
                    State.UpdateContext(j);
                    _ = State.Updated();
                    break;
                case Endpoint.SetQueue:
                    break;
                case Endpoint.AddToQueue:
                    _ = HandleAddToQueue(data.Obj);
                    break;
                case Endpoint.Transfer:
                    await HandleTransferState(TransferState.Parser.ParseFrom(data.Data));
                    State.SpotifyDevice.DeviceChange(_session.DeviceId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null);
            }
        }
        private async Task HandlePause()
        {
            if (!State.IsPaused())
            {
                State.SetState(true, true, false);
                //Sink.Pause(false);
                _ = Device.Pause(false);

                State.SetPosition(State.GetPosition());

                await State.Updated();
                events.PlaybackPaused();
            }
        }
        private async Task HandleResume()
        {
            if (State.IsPaused())
            {
                State.SetState(true, false, false);
                await Device.Resume();

                await State.Updated();
                await events.PlaybackResumed();
            }
        }
        private async Task HandleSkipNext([CanBeNull] JObject obj, 
            [NotNull] TransitionInfo trans)
        {
            ContextTrack track = null;
            if (obj != null) track = PlayCommandHelper.GetTrack(obj);

            if (track != null)
            {
                State.SkipTo(track);
                await LoadTrack(true, TransitionInfo.SkipTo(State));
                return;
            }

            var next = State.NextPlayable(AutoPlay);
            if (next == Spotify.NextPlayable.Autoplay)
            {
                await LoadAutoplay();
                return;
            }

            if (next.IsOk())
            {
                trans.EndedWhen = State.GetPosition();

                State.SetPosition(0);
                await LoadTrack(next == Spotify.NextPlayable.OkPlay || next == Spotify.NextPlayable.OkRepeat, trans);
            }
            else
            {
                Debug.WriteLine("Failed loading next song: " + next);
               // panicState(PlaybackMetrics.Reason.END_PLAY);
            }
        }

        private async Task HandleSkipPrev()
        {
            if (State.GetPosition() < 3000)
            {
                var prev = State.PreviousPlayable();
                if (prev.IsOk())
                {
                    State.SetPosition(0);
                    await LoadTrack(true, TransitionInfo.SkippedPrev(State));
                }
                else
                {
                    Debug.WriteLine("Failed loading previous song: " + prev);
                }
            }
            else
            {
                await PlayerSession.SeekCurrent(0);
                State.SetPosition(0);
                await State.Updated();
            }
        }
        private async Task LoadAutoplay()
        {
            var context = State.ConnectState.ContextUri;
            if (context == null)
            {
                Debug.WriteLine("Cannot load autoplay with null context!");
                //panicState(null);
                return;
            }

            if (context.StartsWith("spotify:search:"))
            {
                Debug.WriteLine("Cannot load autoplay for search context: " + context);

                State.SetPosition(0);
                State.SetState(true, false, false);
                await State.Updated();
                return;
            }
            var contextDesc = State.ConnectState.ContextMetadata["context_description"];
            try
            {
                var resp = _session.Mercury().SendSync(MercuryRequests.AutoplayQuery(context));
                switch (resp.StatusCode)
                {
                    case 200:
                    {
                        var newContext = resp.Payload.ReadIntoString(0);
                        var sessionId = await State.LoadContext(newContext);
                        State.ConnectState.ContextMetadata["context_description"] = contextDesc;

                        events.ContextChanged();
                        await LoadSession(sessionId, true, false);

                        Debug.WriteLine($"Loading context for autoplay, uri: {newContext}");
                        break;
                    }
                    case 204:
                    {
                        var stationRequestResponse = _session.Mercury().SendSync(MercuryRequests.GetStationFor(context));
                        var obj = JObject.Parse(stationRequestResponse);
                        var tracksAr = obj["tracks"] as JArray;
                        var sessionId = await State.LoadContextWithTracks(obj["uri"].ToString(), ProtoUtils.JsonToContextTracks(tracksAr));
                        State.ConnectState.ContextMetadata["context_description"] =  contextDesc;

                        events.ContextChanged();
                        await LoadSession(sessionId, true, false);

                        Debug.WriteLine(
                            $"Loading context for autoplay (using radio-apollo), uri: {State.ConnectState.ContextUri}");
                        break;
                    }
                    default:
                        Debug.WriteLine("Failed retrieving autoplay context, code: " + resp.StatusCode);

                        State.SetPosition(0);
                        State.SetState(true, false, false);
                        await State.Updated();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed loading autoplay station!", ex);
                //panicState(null);
            }
        }
        private async Task HandleAddToQueue([NotNull] JObject obj)
        {
            var track = PlayCommandHelper.GetTrack(obj);
            if (track == null) throw new ArgumentNullException(nameof(track));

            State.AddToQueue(track);
            await State.Updated();
        }

        private async Task HandleSeek(int pos)
        {
            await PlayerSession.SeekCurrent(pos);
            State.SetPosition(pos);
            events.Seeked(pos);
        }

        private async Task HandlePlay([NotNull] JObject obj)
        {
            Debug.WriteLine($"Loading context (play), uri: {PlayCommandHelper.GetContextUri(obj)}");

            try
            {
                var sessionId = await State.Load(obj);
                events.ContextChanged();
                var paused = PlayCommandHelper.IsInitiallyPaused(obj) ?? true;
                await LoadSession(sessionId, !paused, PlayCommandHelper.WillSkipToSomething(obj));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed loading context!" + ex.ToString());
            }
        }

        public async Task Load([NotNull] string uri, bool play)
        {
            try
            {
                var sessionId = await State.LoadContext(uri);
                events.ContextChanged();

                await LoadSession(sessionId, play, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot play local tracks! " + ex.ToString());
            }
        }


        private async Task HandleTransferState([NotNull] TransferState cmd)
        {
            Debug.WriteLine("Loading context (transfer), uri: {}", cmd.CurrentSession.Context.Uri);

            try
            {
                var sessionId = await State.Transfer(cmd);
                events.ContextChanged();
                await LoadSession(sessionId, !cmd.Playback.IsPaused, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed loading context!" + ex.ToString());
            }
        }

        /// <summary>
        /// Loads a new session by creating a new <see cref="PlayerSession"/>. Will also invoke <seealso cref="LoadTrack"/>/>
        /// </summary>
        private async Task LoadSession([NotNull] string sessionId, bool play, bool withSkip)
        {
            var trans = TransitionInfo.ContextChange(State, withSkip);

            if (PlayerSession != null)
            {
                PlayerSession.Dispose();
                PlayerSession = null;
            }

            PlayerSession = new SpotifyPlayerSession(_session,
                Device,
                sessionId,
                this);
            _session.EventService().SendEvent(new NewSessionIdEvent(sessionId, State)
                .Build());

            await LoadTrack(play, trans);
        }

        private async Task LoadTrack(bool play,
            TransitionInfo trans)
        {
            var playbackId = await PlayerSession.Play(State.GetCurrentPlayableOrThrow(),
                State.GetPosition(), trans.StartedReason);
            State.ConnectState.PlaybackId = playbackId;
            _session.EventService()
                .SendEvent(new NewPlaybackIdEvent(State.ConnectState.SessionId,
                    playbackId).Build());

            try
            {
                if (play) await Device.Play();
               else await Device.Pause(false);
            }
            catch (Exception)
            {
                //ignore
            }

            State.SetState(true, !play, true);
            await State.Updated();
            events.TrackChanged();
            if (play) await events.PlaybackResumed();
            else events.PlaybackPaused();
            State.SpotifyDevice.OnCurrentlyPlayingChanged(null);
        }

        public void VolumeChanged()
        {
            throw new NotImplementedException();
        }

        public void NotActive()
        {
            throw new NotImplementedException();
        }

        public IPlayableId CurrentPlayable() => State.GetCurrentPlayableOrThrow();

        public IPlayableId NextPlayable()
        {
            var next = State.NextPlayable(AutoPlay);
            if (next == Spotify.NextPlayable.Autoplay)
            {
             //   LoadAutoplay();
                return null;
            }

            if (next.IsOk())
            {
                if (next != Spotify.NextPlayable.OkPlay && next != Spotify.NextPlayable.OkRepeat)
                    Device.Pause(false);

                return State.GetCurrentPlayableOrThrow();
            }
            else
            {
                Debug.WriteLine("Failed loading next song: " + next);
                //panicState(PlaybackMetrics.Reason.END_PLAY);
                return null;
            }
        }

        public IPlayableId NextPlayableDoNotSet()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> MetadataFor(ISpotifyId playable)
        {
            throw new NotImplementedException();
        }

        public async Task PlaybackHalted()
        {
            Debug.WriteLine("Playback halted on retrieving");
            State.SetBuffering(true);
            await State.Updated();

            events.PlaybackHaltStateChanged(true);
        }

        public async Task PlaybackResumedFromHalt(long diff)
        {
            Debug.WriteLine("Playback resumed, retrieved, took {0}ms.", diff);
            State.SetPosition(diff);
            State.SetBuffering(false);
            await State.Updated();
            events.PlaybackHaltStateChanged(false);
        }

        public async Task StartedLoading()
        {
            if (!State.IsPaused())
            {
                State.SetBuffering(true);
                await State.Updated();
            }
        }

        public void LoadingError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public async Task FinishedLoading(TrackOrEpisode metadata)
        {
            State.EnrichWithMetadata(metadata);
            State.SetBuffering(false);
            await State.Updated();

            await events.MetadataAvailableAsync();
        }

        public void PlaybackError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public async Task TrackChanged(string playbackId, TrackOrEpisode metadata, int pos, Reason startedReason)
        {
            if (metadata != null) State.EnrichWithMetadata(metadata);
            State.ConnectState.PlaybackId = playbackId;
            State.SetPosition(pos);
            await State.Updated();

            events.TrackChanged();
            await events.MetadataAvailableAsync();

            _session.EventService().SendEvent(new NewPlaybackIdEvent(State.ConnectState.SessionId, playbackId).Build());
        }

        public void TrackPlayed(string playbackId, Reason endReason, int endedAt)
        {
            //Ignore
        }

        public void SinkError(Exception ex)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Getters

        public async Task<int> Time()
        {
            try
            {
                int return1 = 0;
                return return1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        #endregion


        #region Remote

        public async Task RequestPlay(RemoteRequest request)
        {
            if (State.SpotifyDevice.ActiveDeviceId == null)
            {
                var t = await _session.Api().Player.GetCurrentPlayback(_session.Locale);
                State.SpotifyDevice.ActiveDeviceId = t.Device?.Id;
            }
            var resp = 
                await _session.Api().ConnectState.TransferState(_session.DeviceId, 
                    State.SpotifyDevice.ActiveDeviceId, request);
            if (!resp.IsSuccessStatusCode)
            {
                Debugger.Break();
            }
        }

        #endregion
    }
}
