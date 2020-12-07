using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Connectstate;
using Flurl.Http;
using GuardAgainstLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Ids;
using SpotifyLib.Interfaces;
using SpotifyLib.Models;
using SpotifyLib.Services;
using SpotifyLib.SpotifyConnect.Contexts;
using SpotifyLib.SpotifyConnect.Handlers;
using SpotifyLib.SpotifyConnect.Helpers;
using SpotifyLib.SpotifyConnect.Models;
using SpotifyLib.SpotifyConnect.Spotify;
using Context = Spotify.Player.Proto.Context;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using PlayerState = Connectstate.PlayerState;
using Restrictions = Connectstate.Restrictions;

namespace SpotifyLib.SpotifyConnect
{
    public class SpotifyState : IDeviceStateHandlerListener, IMessageListener
    {
        public readonly PlayerState ConnectState;
        private readonly SpotifyPlayer _player;
        public readonly DeviceStateHandler SpotifyDevice;

        public readonly SpotifySession Session;
        public AbsSpotifyContext Context;
        public PagesLoader Pages;
        private TracksKeeper tracksKeeper;


        public SpotifyState(
            SpotifySession session,
            SpotifyPlayer player,
            uint initialVolume,
            int volumeSteps)
        {
            Session = session;
            this._player = player;
            this.SpotifyDevice = new DeviceStateHandler(session, initialVolume, volumeSteps);
            this.ConnectState = InitState();

            SpotifyDevice.AddListener(this);
            session.Dealer().AddMessageListener(this, 
                "spotify:user:attributes:update", 
                "hm://playlist/", 
                "hm://collection/collection/" + session.Username + "/json");

        }
        public bool IsShufflingContext() => ConnectState.Options.ShufflingContext;
        public bool IsRepeatingContext() => ConnectState.Options.RepeatingContext;
        public bool IsRepeatingTrack() => ConnectState.Options.RepeatingTrack;

        public IPlayableId GetCurrentPlayable() =>
            ConnectState.Track == null ? null : PlayableId.From(ConnectState.Track);
        public IPlayableId GetCurrentPlayableOrThrow()
        {
            var id = GetCurrentPlayable();
            GuardAgainst.ArgumentBeingNull(id);
            return id;
        }
        public PreviousPlayable PreviousPlayable()
        {
            if (tracksKeeper == null) return Spotify.PreviousPlayable.MISSING_TRACKS;
            return tracksKeeper.PreviousPlayable();
        }
        public async Task<string> LoadContext([NotNull] String uri)
        {
            ConnectState.PlayOrigin = new Connectstate.PlayOrigin();
            ConnectState.Options = new ContextPlayerOptions();
            var sessionId = SetContext(uri);
            tracksKeeper.InitializeStart();
            SetPosition(0);
            await LoadTransforming();
            return sessionId;
        }
        public void SkipTo([NotNull] ContextTrack track)
        {
            tracksKeeper.SkipTo(track);
            SetPosition(0);
        }
        public async Task<string> LoadContextWithTracks([NotNull] string uri, [NotNull] List<ContextTrack> tracks) 
        {
            ConnectState.PlayOrigin = new Connectstate.PlayOrigin();
            ConnectState.Options = new ContextPlayerOptions();

            var sessionId = SetContext(uri);
            Pages.PutFirstPage(tracks);
            tracksKeeper.InitializeStart();
            SetPosition(0);
            await LoadTransforming();
            return sessionId;
        }

        public async Task<string> Transfer([NotNull] TransferState cmd)
        {
            var ps = cmd.CurrentSession;
            ConnectState.PlayOrigin = ProtoUtils.ConvertPlayOrigin(ps.PlayOrigin);
            ConnectState.Options = ProtoUtils.ConvertPlayerOptions(cmd.Options);
            var sessionId = SetContext(ps.Context);

            var pb = cmd.Playback;
            tracksKeeper.InitializeFrom(list => list.FindIndex(z=> z.Uid == ps.CurrentUid), pb.CurrentTrack, cmd.Queue);

            ConnectState.PositionAsOfTimestamp = pb.PositionAsOfTimestamp;
            ConnectState.Timestamp = pb.IsPaused 
                ? TimeProvider.CurrentTimeMillis() : pb.Timestamp;

            await LoadTransforming();
            return sessionId;
        }

        private static PlayerState InitState()
        {
            return new PlayerState
            {
                PlaybackSpeed = 1.0,
                SessionId = string.Empty,
                PlaybackId = string.Empty,
                ContextRestrictions = new Restrictions(),
                Options = new ContextPlayerOptions
                {
                    RepeatingContext = false,
                    ShufflingContext = false,
                    RepeatingTrack = false
                },
                PositionAsOfTimestamp = 0,
                Position = 0,
                IsPlaying = false
            };
        }

        public void EnrichWithMetadata([NotNull] TrackOrEpisode metadata)
        {
            if (metadata.track != null)
            {
                var track = metadata.track;
                GuardAgainst.ArgumentBeingNull(ConnectState.Track);

                if (track.HasDuration) tracksKeeper.UpdateTrackDuration(track.Duration);

                var b = ConnectState.Track;

                if (track.HasPopularity) b.Metadata["popularity"] = track.Popularity.ToString();
                if (track.HasExplicit) b.Metadata["is_explicit"] = track.Explicit.ToString().ToLower();
                if (track.HasName) b.Metadata["title"] = track.Name;
                if (track.HasDiscNumber) b.Metadata["album_disc_number"] = track.DiscNumber.ToString();
                for (var i = 0; i < track.Artist.Count; i++)
                {
                    var artist = track.Artist[i];
                    if (artist.HasName) b.Metadata["artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                    if (artist.HasGid)
                        b.Metadata["artist_uri" + (i == 0 ? "" : (":" + i))] =
                            new ArtistId(Utils.bytesToHex(artist.Gid.ToByteArray())).Uri;
                }

                if (track.Album != null)
                {
                    var album = track.Album;
                    if (album.Disc.Count > 0)
                    {
                        b.Metadata["album_track_count"] = album.Disc.Select(z => z.Track).Count().ToString();
                        b.Metadata["album_disc_count"] = album.Disc.Count.ToString();
                    }

                    if (album.HasName) b.Metadata["album_title"] = album.Name;
                    if (album.HasGid)
                        b.Metadata["album_uri"] =
                            AlbumId.FromHex(Utils.bytesToHex(album.Gid.ToByteArray())).Uri;

                    for (int i = 0; i < album.Artist.Count; i++)
                    {
                        var artist = album.Artist[i];
                        if (artist.HasName)
                            b.Metadata["album_artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                        if (artist.HasGid)
                            b.Metadata["album_artist_uri" + (i == 0 ? "" : (":" + i))] =
                                ArtistId.FromHex(Utils.bytesToHex(artist.Gid.ToByteArray())).Uri;
                    }

                    if (track.HasDiscNumber)
                    {
                        b.Metadata["album_track_number"] =
                            album.Disc.SelectMany(z => z.Track).ToList().FindIndex(k => k.Gid == track.Gid) +
                            1.ToString();
                    }

                    if (album.CoverGroup?.Image != null
                        && album.CoverGroup.Image.Count > 0)
                        ImageId.PutAsMetadata(b, album.CoverGroup);
                }

                var k = new JArray();
                foreach (var j in track.File
                    .Where(z => z.HasFormat))
                {
                    k.Add(j.Format.ToString());
                }

                b.Metadata["available_file_formats"] = k.ToString();
                ConnectState.Track = b;
            }
        }
        private string SetContext([NotNull] string uri)
        {
            this.Context = AbsSpotifyContext.From(uri);
            this.ConnectState.ContextUri = uri;

            if (!Context.IsFinite())
            {
                SetRepeatingContext(false);
                SetShufflingContext(false);
            }

            this.ConnectState.ContextUrl = "";
            this.ConnectState.Restrictions = null;
            this.ConnectState.ContextRestrictions = null;
            this.ConnectState.ContextMetadata.Clear();
            this.Pages = PagesLoader.From(Session, uri);
            this.tracksKeeper = new TracksKeeper(this);

            this.SpotifyDevice.SetIsActive(true);
            return RenewSessionId();
        }
        private string SetContext([NotNull] Context ctx)
        {
            var uri = ctx.Uri;
            Context = AbsSpotifyContext.From(uri);
            ConnectState.ContextUri = uri;

            if (!Context.IsFinite())
            {
                SetRepeatingContext(false);
                SetShufflingContext(false);
            }

            if (ctx.HasUrl) this.ConnectState.ContextUrl = ctx.Url;
            else this.ConnectState.ContextUrl = String.Empty;

            ConnectState.ContextMetadata.Clear();
            ProtoUtils.CopyOverMetadata(ctx, ConnectState);

            this.Pages = PagesLoader.From(Session, ctx);
            this.tracksKeeper = new TracksKeeper(this);

            this.SpotifyDevice.SetIsActive(true);

            return RenewSessionId();
        }
        private string RenewSessionId()
        {
            String sessionId = GenerateSessionId();
            ConnectState.SessionId = sessionId;
            return sessionId;
        }
        public async Task Ready()
        {
            ConnectState.IsSystemInitiated = true;
            await SpotifyDevice.UpdateState(PutStateReason.NewDevice, -1, ConnectState);
        }

        public async Task Command(Endpoint endpoint, CommandBody data)
        {
            throw new NotImplementedException();
        }

        public void VolumeChanged()
        {
            throw new NotImplementedException();
        }

        public void NotActive()
        {
            throw new NotImplementedException();
        }

        public async Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            throw new NotImplementedException();
        }

        public void AddToQueue([NotNull] ContextTrack track)
        {
            tracksKeeper.AddToQueue(track);
        }

        public void AddListener([NotNull] IDeviceStateHandlerListener listener)
        {
            SpotifyDevice.AddListener(listener);
        }
        private void UpdateRestrictions()
        {
            if (Context == null) return;

            if (tracksKeeper.IsPlayingFirst() && !IsRepeatingContext())
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_PREV, RestrictionsManager.REASON_NO_PREV_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_PREV);

            if (tracksKeeper.IsPlayingLast() && !IsRepeatingContext())
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_NEXT, RestrictionsManager.REASON_NO_NEXT_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_NEXT);

            ConnectState.Restrictions = Context.Restrictions.ToProto();
            ConnectState.ContextRestrictions = Context.Restrictions.ToProto();
        }

        public async Task Updated()
        {
            UpdateRestrictions();
            await SpotifyDevice.UpdateState(PutStateReason.PlayerStateChanged, await _player.Time(), ConnectState);
        }

        #region Setters
        public void SetBuffering(bool buffering)
        {
            SetState(true, ConnectState.IsPaused, buffering);
        }
        public void SetState(bool playing,
            bool paused, 
            bool buffering)
        {
            if (paused && !playing) throw new Exception("illegal state");
            else if (buffering && !playing) throw new Exception("illegal state");

            var wasPaused = IsPaused();
            ConnectState.IsPlaying = playing;
            ConnectState.IsPaused = paused;
            ConnectState.IsBuffering = buffering;
            if (paused)
            {
                _player.Device.Pause(false);
            }
            else
            {
                _player.Device.Play();
            }

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(ConnectState.PositionAsOfTimestamp);
        }

        public void SetPosition(long pos)
        {
            ConnectState.Timestamp = TimeProvider.CurrentTimeMillis();
            ConnectState.PositionAsOfTimestamp = pos;
            SpotifyDevice.OnPositionChanged(pos);
            _player.Device.Position = TimeSpan.FromMilliseconds(pos);
            ConnectState.Position = 0L;
        }

        public void SetShufflingContext(bool value)
        {
            if (Context == null) return;

            var old = IsShufflingContext();
            ConnectState.Options.ShufflingContext =
                value && Context.Restrictions.Can(RestrictionsManager.Action.SHUFFLE);

            if (old != IsShufflingContext()) tracksKeeper.ToggleShuffle(IsShufflingContext());
        }
        public void SetRepeatingContext(bool value)
        {
            if (Context == null) return;
            ConnectState.Options.RepeatingContext =
                value && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }
        public void SetRepeatingTrack(bool value)
        {
            if (Context == null) return;
            ConnectState.Options.RepeatingTrack =
                value && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_TRACK);
        }
        public async Task<string> Load([NotNull] JObject obj)
        {
            var k = (PlayCommandHelper.GetPlayOrigin(obj) as JObject);
            ConnectState.PlayOrigin = ProtoUtils.JsonToPlayOrigin(k!);
            ConnectState.Options =
                ProtoUtils.JsonToPlayerOptions((JObject)PlayCommandHelper.GetPlayerOptionsOverride(obj), ConnectState.Options);
            var sessionId = SetContext(ProtoUtils.JsonToContext((JObject) PlayCommandHelper.GetContext(obj)));

            var trackUid = PlayCommandHelper.GetSkipToUid(obj);
            var trackUri = PlayCommandHelper.GetSkipToUri(obj);
            var trackIndex = PlayCommandHelper.GetSkipToIndex(obj);

            if (trackUri != null)
            {
                tracksKeeper.InitializeFrom(list => list.FindIndex(z=> z.Uri == trackUri), null, null);
            }
            else if (trackUid != null)
            {
                tracksKeeper.InitializeFrom(list => list.FindIndex(z => z.Uid == trackUid), null, null);
            }
            else if (trackIndex != null)
            {
                tracksKeeper.InitializeFrom(list =>
                {
                    if (trackIndex < list.Count) return (int) trackIndex;
                    return -1;
                }, null, null);
            }
            else
            {
                tracksKeeper.InitializeStart();
            }

            var seekTo = PlayCommandHelper.GetSeekTo(obj);
            if (seekTo != null) SetPosition((long) seekTo);
            else SetPosition(0);
            await LoadTransforming();
            return sessionId;
        }

        private async Task LoadTransforming()
        {
            if (tracksKeeper == null) throw new Exception("Illegal State");

            string url = null;
            if (ConnectState.ContextMetadata.ContainsKey("transforming.url"))
                url = ConnectState.ContextMetadata["transforming.url"];
            if (url == null) return;

            var shuffle = false;
            if (ConnectState.ContextMetadata.ContainsKey("transforming.shuffle"))
                shuffle = bool.Parse(ConnectState.ContextMetadata["transforming.shuffle"]);

            var willRequest =
                !ConnectState.Track.Metadata.ContainsKey(
                    "audio.fwdbtn.fade_overlap"); // I don't see another way to do this
            Debug.WriteLine("Context has transforming! url: {0}, shuffle: {1}, willRequest: {2}", url, shuffle,
                willRequest);

            if (!willRequest) return;
            var obj = ProtoUtils.CraftContextStateCombo(ConnectState,
                tracksKeeper.Tracks);
            try
            {
                var body = await url
                    .PostJsonAsync(obj)
                    .ReceiveString();
                if (body != null) UpdateContext(JObject.Parse(body));
                Debug.WriteLine("Updated context with transforming information!");
            }
            catch (FlurlHttpException ex)
            {
                Debug.WriteLine($"Failed loading cuepoints " +
                                $"Error returned from {ex.Call.Request.Url}: {ex.Message}");
                return;
            }
        }

        public NextPlayable NextPlayable(bool autoplayEnabled)
        {
            if (tracksKeeper == null) return Spotify.NextPlayable.MissingTracks;

            try
            {
                return tracksKeeper.NextPlayable(autoplayEnabled);
            }
            catch (Exception x)
            {
                Debug.WriteLine("Failed fetching next playable.", x);
                return Spotify.NextPlayable.MissingTracks;
            }
        }

        public void UpdateContext([NotNull] JObject obj)
        {
            var uri = obj["uri"].ToString();
            if (!Context.Uri().Equals(uri))
            {
                Debug.WriteLine("Received update for the wrong context! context: {0}, newUri: {1}",
                    Context, uri);
                return;
            }

            ProtoUtils.CopyOverMetadata(((JObject)obj["metadata"])!, ConnectState);
            tracksKeeper.UpdateContext(ProtoUtils.JsonToContextPages(((JArray)obj["pages"])!));
        }
        #endregion

        #region Getters


        public bool IsPaused()
        {
            return ConnectState.IsPlaying && ConnectState.IsPaused;
        }
        public int GetContextSize()
        {
            var trackCount = ConnectState.ContextMetadata.ContainsKey("track_count") 
                ? ConnectState.ContextMetadata["track_count"] : null;
            if (trackCount != null) return int.Parse(trackCount);
            return tracksKeeper?.Tracks.Count ?? 0;
        }
        public int GetPosition()
        {
            var diff = (int)(TimeProvider.CurrentTimeMillis() - ConnectState.Timestamp);
            return (int)(ConnectState.PositionAsOfTimestamp + diff);
        }

        #endregion

        #region  Private
        public static string GeneratePlaybackId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            bytes[0] = 1;
            return Utils.bytesToHex(bytes).ToLowerInvariant();
        }
        private static string GenerateSessionId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("-", "");
        }
        #endregion
    }
}
