using System;
using System.Collections.Generic;
using System.Text;
using Connectstate;
using SpotifyLib.Helpers;
using SpotifyLib.Models.Api.Requests;
using SpotifyLib.SpotifyConnect.Handlers;

namespace SpotifyLib.SpotifyConnect
{
    public class ConnectHandler  : IDisposable
    {

        public delegate void SpotifyEventHandler<in T>(
            (ConnectHandler connectHandler, DeviceStateHandler deviceStateHandler) sender,
            T args);

        public SpotifyPlayer Player;
        public ConnectHandler(SpotifySession session, 
            SpotifyPlayer player)
        {
            Player = player;

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
            throw new NotImplementedException();
        }

        private void SpotifyDevice_OnDeviceChanged(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void SpotifyDevice_PositionChanged(object sender, double e)
        {
            throw new NotImplementedException();
        }

        private void SpotifyDevice_CurrentlyPlayingChanged(object sender, SpotifyLib.Models.Api.Requests.PlayingChangedRequest e)
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
