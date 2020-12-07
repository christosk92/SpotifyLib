using System;
using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Listeners
{
    public interface IAudioSinkListener
    {
        void SinkError([NotNull] Exception ex);
    }
}
