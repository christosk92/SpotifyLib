using JetBrains.Annotations;
using SpotifyLib.Mercury;

namespace SpotifyLib.Interfaces
{
    public interface ICallback
    {
        internal void Response([NotNull] MercuryResponse response);
    }
}

