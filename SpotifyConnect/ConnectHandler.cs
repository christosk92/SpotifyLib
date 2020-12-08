using System;
using System.Collections.Generic;
using System.Text;
using Connectstate;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.SpotifyConnect.Handlers;

namespace SpotifyLib.SpotifyConnect
{
    public class DeviceChanged
    {
        internal DeviceChanged(string deviceId, bool isOwnDevice)
        {
            NewDeviceId = deviceId;
            IsOwnDevice = isOwnDevice;
        }
        public string NewDeviceId { get; }
        public bool IsOwnDevice { get; }
    }
    public class PositionChanged
    {
        internal PositionChanged(StreamingContext context,
            double to)
        {
            From = context;
            SeekTo = to;
        }
        public StreamingContext From { get; }
        public double SeekTo { get; }
    }

    public class ConnectHandler  : IDisposable
    {

        public delegate void SpotifyEventHandler<in T>(
            (ConnectHandler connectHandler, DeviceStateHandler deviceStateHandler) sender,
            T args);

        public SpotifyPlayer Player;
        private readonly SpotifySession session;
        public ConnectHandler(SpotifySession session, 
            SpotifyPlayer player)
        {
            Player = player;
            this.session = session;
            this.Player.State.SpotifyDevice.CurrentlyPlayingChanged 
                += SpotifyDevice_CurrentlyPlayingChanged;
            this.Player.State.SpotifyDevice.PositionChanged += SpotifyDevice_PositionChanged;
            this.Player.State.SpotifyDevice.OnDeviceChanged += SpotifyDevice_OnDeviceChanged;
            this.Player.State.SpotifyDevice.PauseChanged += SpotifyDeviceOnPauseChanged;
            this.Player.State.SpotifyDevice.ShuffleStateChanged += SpotifyDeviceOnShuffleStateChanged;
            this.Player.State.SpotifyDevice.RepeatStateChanged += SpotifyDeviceOnRepeatStateChanged;
            _ = Singleton<SpotifySession>.Instance.Dealer().Connect();
        }

        /// <summary>
        /// Wil get invoked when the repeat state of the device has changed.
        ///  enum: <seealso cref="PlayerSetRepeatRequest.RepeatState"/>
        /// </summary>
        public virtual event SpotifyEventHandler<PlayerSetRepeatRequest.RepeatState> RepeatStateChanged;

        /// <summary>
        /// Will get invoked when the shuffle state of the device has changed.
        /// </summary>
        public virtual event SpotifyEventHandler<bool> ShuffleStateChanged;

        public virtual event SpotifyEventHandler<StreamingContext> PauseRequested;
        public virtual event SpotifyEventHandler<StreamingContext> ResumeRequested;
        public virtual event SpotifyEventHandler<DeviceChanged> OnDeviceChanged;
        public virtual event SpotifyEventHandler<PositionChanged> PositionChanged;
        //public virtual event SpotifyEventHandler<StreamingContext> PauseRequested;

        private void SpotifyDeviceOnRepeatStateChanged(object sender, 
            PlayerSetRepeatRequest.RepeatState state)
        {
            RepeatStateChanged?.Invoke((this, sender as DeviceStateHandler), state);
        }

        private void SpotifyDeviceOnShuffleStateChanged(object sender, bool e)
        {
            ShuffleStateChanged?.Invoke((this, sender as DeviceStateHandler), e);
        }

        private void SpotifyDeviceOnPauseChanged(object sender, bool e)
        {
            if (e)
            {
                PauseRequested?.Invoke((this, sender as DeviceStateHandler), );
            }
            else
            {
                ResumeRequested?.Invoke((this, sender as DeviceStateHandler), );
            }
        }

        private void SpotifyDevice_OnDeviceChanged(object sender, string e)
        {
            OnDeviceChanged?.Invoke((this, sender as DeviceStateHandler), new DeviceChanged(e, 
                session.DeviceId == e));
        }

        private void SpotifyDevice_PositionChanged(object sender, double e)
        {
            var state = Player.State.SpotifyDevice.ActiveDeviceId == session.DeviceId
                ? StreamingContext.Device
                : StreamingContext.Connect;
            PositionChanged?.Invoke((this, sender as DeviceStateHandler),
                new PositionChanged(state, e));
        }

        private void SpotifyDevice_CurrentlyPlayingChanged(object sender, 
            SpotifyLib.Models.Api.Requests.PlayingChangedRequest e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Player.State.SpotifyDevice.CurrentlyPlayingChanged
                -= SpotifyDevice_CurrentlyPlayingChanged;
            this.Player.State.SpotifyDevice.PositionChanged -= SpotifyDevice_PositionChanged;
            this.Player.State.SpotifyDevice.OnDeviceChanged -= SpotifyDevice_OnDeviceChanged;
            this.Player.State.SpotifyDevice.PauseChanged -= SpotifyDeviceOnPauseChanged;
            this.Player.State.SpotifyDevice.ShuffleStateChanged -= SpotifyDeviceOnShuffleStateChanged;
            this.Player.State.SpotifyDevice.RepeatStateChanged -= SpotifyDeviceOnRepeatStateChanged;
            Dispose(true);
        }

        public virtual void Dispose(bool val)
        {

        }
    }
}
