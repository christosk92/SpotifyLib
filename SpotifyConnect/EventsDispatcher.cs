using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Interfaces;

namespace SpotifyLib.SpotifyConnect
{
    public class EventsDispatcher
    {
     //   private readonly MetadataPipe metadataPipe;
        public readonly List<IEventsListener> Listeners = new List<IEventsListener>();
        private readonly SpotifyPlayer _player;

        public EventsDispatcher(
            [NotNull] SpotifyPlayer player)
        {
          //  metadataPipe = new MetadataPipe(conf);
            _player = player;
        }

        public async Task SendImage()
        {
            //TODO: 
            byte[] image = null;
            try
            {
                //image = CurrentCoverImage();
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Failed downloading image.", ex);
                return;
            }

            if (image == null)
            {
                Debug.WriteLine("No image found in metadata.");
                return;
            }

         //   await metadataPipe.SafeSend(MetadataPipe.TYPE_SSNC, MetadataPipe.CODE_PICT, image);
        }
        public void SendProgress()
        {
            var metadata = _player.CurrentMetadata();
            if (metadata == null) return;
            /*  var data =
                  $"1/{_player.State.GetPosition() * _player.Sink.DEFAULT_FORMAT.SampleRate / 1000 + 1}.0f/{metadata.Duration * _player.Sink.DEFAULT_FORMAT.SampleRate / 1000 + 1}.0f";
              metadataPipe.SafeSend(MetadataPipe.TYPE_SSNC, MetadataPipe.CODE_PRGR, data);*/
        }
        public async Task SendTrackInfo()
        {
            var metadata = _player.CurrentMetadata();
            if (metadata == null) return;

          //  await metadataPipe.SafeSend(MetadataPipe.TYPE_CORE, MetadataPipe.CODE_MINM, metadata.track?.Name ?? metadata.episode.Name);
           // await metadataPipe.SafeSend(MetadataPipe.TYPE_CORE, MetadataPipe.CODE_ASAL, metadata.track?.Album.Name ?? metadata.episode.Show.Name);
           // await metadataPipe.SafeSend(MetadataPipe.TYPE_CORE, MetadataPipe.CODE_ASAR, metadata.track?.Artist[0].Name ?? metadata.episode.Show.Name);
        }
        public async Task SendVolume(int value)
        {
            float xmlValue;
            if (value == 0) xmlValue = 144.0f;
            else xmlValue = (value - SpotifyPlayer.VolumeMax) * 30.0f / (SpotifyPlayer.VolumeMax - 1);
            var volData = $"{xmlValue}.2f,0.00,0.00,0.00";
         //   await metadataPipe.SafeSend(MetadataPipe.TYPE_SSNC, MetadataPipe.CODE_PVOL, volData);
        }
        public void PlaybackPaused()
        {
            long trackTime = _player.State.GetPosition();
            Listeners.ForEach(z => z.OnPlaybackPaused(trackTime));
        }
        public async Task PlaybackResumed()
        {
            long trackTime = _player.State.GetPosition();
            Listeners.ForEach(l => l.OnPlaybackResumed(trackTime));
            //if (!metadataPipe.IsEnabled) return;
            await SendTrackInfo();
            SendProgress();
            await SendImage();
        }
        public void ContextChanged()
        {
            var uri = _player.State.ConnectState.ContextUri;
            if (uri == null) return;

            Listeners.ForEach(z => z.OnContextChanged(uri));
        }
        public void TrackChanged()
        {
            var id = _player.CurrentPlayable();
            if (id == null) return;

            var metadata = _player.CurrentMetadata();
            foreach (var j in Listeners)
            {
                j.OnTrackChanged(id, metadata);
            }
        }
        public void Seeked(int pos)
        {
            Listeners.ForEach(z => z.OnTrackSeeked(pos));
            //if (metadataPipe.IsEnabled) SendProgress();
        }
        public async Task VolumeChanged(int value)
        {
            var volume = (float)value / SpotifyPlayer.VolumeMax;
            Listeners.ForEach(z => z.OnVolumeChanged(volume));

            //if (metadataPipe.IsEnabled) await SendVolume(value);
        }
        public async Task MetadataAvailableAsync()
        {
            var metadata = _player.CurrentMetadata();
            if (metadata == null) return;

            Listeners.ForEach(l => l.OnMetadataAvailable(metadata));

            //if (!metadataPipe.IsEnabled) return;
            await SendTrackInfo();
            SendProgress();
            SendImage();
        }

        public void PlaybackHaltStateChanged(bool halted) =>
            Listeners.ForEach(z => z.OnPlaybackHaltStateChanged(halted, _player.State.GetPosition()));

        public void InactiveSession(bool timeout) => Listeners.ForEach(z => z.OnInactiveSession(timeout));

    }
}

