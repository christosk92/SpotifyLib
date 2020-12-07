using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLib.Enums;
using SpotifyLib.Ids;
using SpotifyLib.Models;
using SpotifyLib.SpotifyConnect.Models;
using SpotifyLib.SpotifyConnect.Spotify;
using SpotifyProto;

namespace SpotifyLib.SpotifyConnect
{
    public interface ISpotifyDevice : IDisposable
    {
        TimeSpan Position { get; set; }
        MediaPlaybackState PlaybackState { get; set; }
        event EventHandler<object> PlaybackStateChanged;
        event EventHandler<object> MediaOpened;
        event EventHandler<object> MediaEnded;

        Task Pause();
        Task Pause(bool v);
        Task Resume();
        Task Play();
        Task<(Track track, Episode episode)> Load(IPlayableId id, bool preloaded, PlayerQueueEntry entry, int initialSeek);
    }
}
