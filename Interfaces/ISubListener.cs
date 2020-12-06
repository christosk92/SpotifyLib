using JetBrains.Annotations;
using SpotifyLib.Mercury;

namespace SpotifyLib.Interfaces
{
    public interface ISubListener
    {
        void OnEvent([NotNull] MercuryResponse resp);
    }
}
