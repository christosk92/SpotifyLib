using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using GuardAgainstLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Ids;
using SpotifyLib.Interfaces;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.Services;
using SpotifyLib.SpotifyConnect.Models;
using PlayerState = Connectstate.PlayerState;

namespace SpotifyLib.SpotifyConnect.Handlers
{
    public class DeviceStateHandler : IMessageListener, IRequestListener
    {
        #region Std

        private readonly DeviceInfo _deviceInfo;

        private readonly object listenersLock = new object();
        private readonly List<IDeviceStateHandlerListener> listeners = new
            List<IDeviceStateHandlerListener>();

        private readonly PutStateRequest _putState;
        private volatile string _connectionId = null;
        public string ActiveDeviceId;
        private readonly SpotifySession _session;

        public DeviceStateHandler(
            [NotNull] SpotifySession session,
            uint initialVolume,
            int volumeSteps)
        {
            previousPause = null;
               _session = session;
            this._deviceInfo = InitializeDeviceInfo(initialVolume, volumeSteps);
            this._putState = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            session.Dealer().AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
            session.Dealer().AddRequestListener(this,
                "hm://connect-state/v1/");
        }

        private DeviceInfo InitializeDeviceInfo(uint initialVolume, int volumeSteps)
            => new DeviceInfo
            {
                CanPlay = true,
                Volume = initialVolume,
                Name = _session.DeviceName,
                DeviceId = _session.DeviceId,
                DeviceType = (DeviceType) _session.DeviceType,
                DeviceSoftwareVersion = "Spotify-11.1.0",
                SpircVersion = "3.2.6",
                Capabilities = new Capabilities
                {
                    CanBePlayer = true,
                    GaiaEqConnectId = true,
                    SupportsLogout = true,
                    IsObservable = true,
                    CommandAcks = true,
                    SupportsRename = true,
                    SupportsTransferCommand = true,
                    SupportsCommandRequest = true,
                    VolumeSteps = volumeSteps,
                    SupportsGzipPushes = true,
                    NeedsFullPlayerState = true,
                    SupportedTypes =
                    {
                        new List<string>
                        {
                            {"audio/episode"},
                            {"audio/track"}
                        }
                    }
                }
            };

        public void AddListener([NotNull] IDeviceStateHandlerListener listener)
        {
            lock (listenersLock)
            {
                listeners.Add(listener);
            }
        }

        public void RemoveListener([NotNull] IDeviceStateHandlerListener listener)
        {
            lock (listenersLock)
            {
                listeners.Remove(listener);
            }
        }

        #endregion

        #region Events

        public void DeviceChange(string deviceId)
        {
            OnDeviceChanged?.Invoke(this, deviceId);
        }
        public void OnPositionChanged(double pos)
        {
            PositionChanged?.Invoke(this, pos);
        }
        public void OnContextUpdate(List<ContextTrack> context)
        {
            ContextUpdate?.Invoke(this, context);
        }
        public void OnQueueSet(object trackUri)
        {
            SetQueue?.Invoke(this, trackUri);
        }
        public void OnQueueAdd(ContextTrack trc)
        {
            AddToQueue?.Invoke(this, trc);
        }
        public void OnSkipNext(NextRequested nxt)
        {
            SkipNext?.Invoke(this, nxt);
        }
        public void OnSkipPrevious(IPlayableId prv)
        {
            SkipPrevious?.Invoke(this, prv);
        }
        public void OnCurrentlyPlayingChanged(PlayingChangedRequest trackUri)
        {
            CurrentlyPlayingChanged?.Invoke(this, trackUri);
        }


        public event EventHandler<double> PositionChanged;
        /// <summary>
        /// If the parameter is set to true. The object has been paused
        /// </summary>
        public event EventHandler<bool> PauseChanged;
        public event EventHandler<PlayingChangedRequest> CurrentlyPlayingChanged;
        public event EventHandler<(List<string> added, List<string> removed)> CollectionChanged;
        public event EventHandler<string> OnDeviceChanged;
        public event EventHandler<PlayerSetRepeatRequest.RepeatState> RepeatStateChanged;
        public event EventHandler<bool> ShuffleStateChanged;
        public event EventHandler<object> SetQueue;
        public event EventHandler<ContextTrack> AddToQueue;
        public event EventHandler<List<ContextTrack>> ContextUpdate;
        public event EventHandler<NextRequested> SkipNext;
        public event EventHandler<IPlayableId> SkipPrevious;
        #endregion

        #region Notifications

        private void NotifyCommand([NotNull] Endpoint endpoint, [NotNull] CommandBody data)
        {
            lock (listenersLock)
            {
                if (!listeners.Any())
                {
                    Debug.WriteLine("Cannot dispatch command because there are no listeners. command: {0}", endpoint);
                    return;
                }
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.Command(endpoint, data);
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine("Failed parsing command!", x);
                    }
                }
            }
        }

        private void NotifyReady()
        {
            lock (listenersLock)
            {
                foreach (var deviceStateHandlerListener in listeners)
                {
                    deviceStateHandlerListener.Ready();
                }
            }
        }

        private void NotifyVolumeChange()
        {
            lock (listenersLock)
            {
                listeners.ToList().ForEach(x => x.VolumeChanged());
            }
        }

        private void NotifyNotActive()
        {
            lock (listenersLock)
            {
                listeners.ToList().ForEach(x => x.NotActive());
            }
        }

        private void UpdateConnectionId([NotNull] string conId)
        {
            try
            {
                conId = HttpUtility.UrlDecode(conId, Encoding.UTF8);
            }
            catch (Exception)
            {
                //ignored
            }

            if (_connectionId != null && _connectionId.Equals(conId)) return;
            _connectionId = conId;
            Debug.WriteLine("Updated Spotify-Connection-Id: " + _connectionId);
            NotifyReady();
        }

        #endregion

        #region Getters & Setters

        public PlayingChangedRequest CurrentCluster;
        public void SetIsActive(bool active)
        {
            if (active)
            {
                if (!_putState.IsActive)
                {
                    long now = TimeProvider.CurrentTimeMillis();
                    _putState.IsActive = true;
                    _putState.StartedPlayingAt = (ulong) now;
                    Debug.WriteLine("Device is now active. ts: {0}", now);
                }
            }
            else
            {
                _putState.IsActive = false;
                _putState.StartedPlayingAt = 0L;
            }
        }

        public uint GetVolume()
        {
            lock (this)
            {
                return _deviceInfo.Volume;
            }
        }

        public void SetVolume(int val)
        {
            lock (this)
            {
                _deviceInfo.Volume = (uint) val;
            }

            NotifyVolumeChange();
            Debug.WriteLine("Update volume. volume: {0}/{1}", val, 100);
        }

        public async Task UpdateState([NotNull] PutStateReason reason,
            int playerTime,
            [NotNull] PlayerState state)
        {
            GuardAgainst.ArgumentBeingNullOrEmpty(_connectionId);

            if (playerTime == -1) _putState.HasBeenPlayingForMs = 0L;
            else _putState.HasBeenPlayingForMs = (ulong) playerTime;

            _putState.PutStateReason = reason;
            _putState.ClientSideTimestamp = (ulong) TimeProvider.CurrentTimeMillis();
            _putState.Device.DeviceInfo = _deviceInfo;
            _putState.Device.PlayerState = state;
            await PutConnectState(_putState);
        }

        #endregion

        #region Spotify
        private ApResolver ap;
        private async Task PutConnectState([NotNull] PutStateRequest req)
        {
            try
            {
                using var htp = new HttpClient
                {
                    BaseAddress = new Uri((await ap.GetClosestSpClient()))
                };
                htp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _session.Tokens().GetToken("playlist-read").AccessToken);
                var byteArrayContent = new ByteArrayContent(req.ToByteArray());
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                htp.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);
                var res = await htp.PutAsync($"/connect-state/v1/devices/{_session.DeviceId}", byteArrayContent);

                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Put new connect state:");
                }
                else
                {
                    Debugger.Break();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed updating state.", ex);
            }
        }
        #endregion

        #region Implementations

        private long previousSet;
        private bool? previousPause;
        public bool? PreviousShuffle;
        public PlayerSetRepeatRequest.RepeatState? PreviousRepeatState;
        public Task OnMessage(string uri,
            Dictionary<string, string> headers,
            byte[] payload)
        {
            switch (uri)
            {
                case { } str when str.StartsWith("hm://pusher/v1/connections/"):
                    UpdateConnectionId(headers["Spotify-Connection-Id"]);
                    break;
                case { } str when str.Equals("hm://connect-state/v1/connect/volume"):
                    var cmd = SetVolumeCommand.Parser.ParseFrom(payload);
                    lock (this)
                    {
                        _deviceInfo.Volume = (uint) cmd.Volume;
                        if (cmd.CommandOptions != null)
                        {
                            _putState.LastCommandMessageId = ((uint) cmd.CommandOptions.MessageId);
                            _putState.LastCommandSentByDeviceId = string.Empty;
                        }
                    }

                    Debug.WriteLine("Update volume. volume: {0}/{1}", cmd.Volume, 100);
                    NotifyVolumeChange();
                    break;
                case { } str when str.StartsWith("hm://connect-state/v1/cluster"):
                    var update = ClusterUpdate.Parser.ParseFrom(payload);

                    if (previousPause != update.Cluster.PlayerState.IsPaused)
                    {
                        previousPause = update.Cluster.PlayerState.IsPaused;
                        PauseChanged?.Invoke(this, update.Cluster.PlayerState.IsPaused);
                    }

                    var now = TimeProvider.CurrentTimeMillis();
                    Debug.WriteLine("Received cluster update at {0}: {1}", now, update);
                    var ts = update.Cluster.Timestamp - 3000; // Workaround
                    try
                    {
                        if (!_session.DeviceId.Equals(update.Cluster.ActiveDeviceId)
                            && _putState.IsActive
                            && (ulong) now > _putState.StartedPlayingAt
                            && (ulong) ts > _putState.StartedPlayingAt)
                            NotifyNotActive();
                    }
                    catch (Exception x)
                    {

                    }

                    var updated = update.Cluster.ActiveDeviceId != ActiveDeviceId;
                    if (updated)
                    {
                        ActiveDeviceId = update.Cluster.ActiveDeviceId;
                        OnDeviceChanged?.Invoke(this, ActiveDeviceId);
                    }
                    if (update?.Cluster?.PlayerState?.PositionAsOfTimestamp != previousSet)
                    {
                        previousSet = update.Cluster.PlayerState.PositionAsOfTimestamp;
                        var diff = (int)(TimeProvider.CurrentTimeMillis() - update.Cluster.PlayerState.Timestamp);
                        Debug.WriteLine("Expected timestamp: " +
                                        (int)(update.Cluster.PlayerState.PositionAsOfTimestamp + diff));
                        OnPositionChanged(update.Cluster.PlayerState.PositionAsOfTimestamp + diff);
                    }

                    if (update.Cluster?.PlayerState.Options != null)
                    {
                        if (update.Cluster.PlayerState.Options.ShufflingContext != PreviousShuffle)
                        {
                            ShuffleStateChanged?.Invoke(this, update.Cluster.PlayerState.Options.ShufflingContext);
                            PreviousShuffle = update.Cluster.PlayerState.Options.ShufflingContext;
                        }

                        var repeatingTrack = update.Cluster.PlayerState.Options.RepeatingTrack;
                        var repeatingContext = update.Cluster.PlayerState.Options.RepeatingContext;
                        if (repeatingContext && !repeatingTrack)
                        {
                            PreviousRepeatState = PlayerSetRepeatRequest.RepeatState.Context;
                            RepeatStateChanged?.Invoke(this, PlayerSetRepeatRequest.RepeatState.Context);
                        }
                        else
                        {
                            if (repeatingTrack)
                            {
                                PreviousRepeatState = PlayerSetRepeatRequest.RepeatState.Track;
                                RepeatStateChanged?.Invoke(this, PlayerSetRepeatRequest.RepeatState.Track);
                            }
                            else
                            {
                                PreviousRepeatState = PlayerSetRepeatRequest.RepeatState.Off;
                                RepeatStateChanged?.Invoke(this, PlayerSetRepeatRequest.RepeatState.Off);
                            }
                        }
                    }

                    var j = new PlayingChangedRequest
                    {
                        ItemUri = update?.Cluster?.PlayerState?.Track?.Uri,
                        IsPaused = update?.Cluster?.PlayerState?.IsPaused,
                        IsPlaying = update?.Cluster?.PlayerState?.IsPlaying,
                        ContextUri = update?.Cluster.PlayerState.ContextUri
                    };
                    CurrentCluster = j;
                    //update?.Cluster?.PlayerState?.Track?.Uri
                    CurrentlyPlayingChanged?.Invoke(this, j);
                    break;
                case { }:
                    Debug.WriteLine("Message left unhandled! uri: {0}", uri);
                    break;

            }

            return Task.FromResult("done");
        }

        public RequestResult OnRequest(string mid, int pid, string sender, JObject command)
        {
            _putState.LastCommandMessageId = (uint) pid;
            _putState.LastCommandSentByDeviceId = sender;
            var endpoint = command["endpoint"].ToString().StringToEndPoint();
            NotifyCommand(endpoint, new CommandBody(command));
            return RequestResult.Success;
        }

        #endregion
    }
}
