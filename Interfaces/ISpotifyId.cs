using SpotifyLib.Enums;

namespace SpotifyLib.Interfaces
{
    public interface ISpotifyDescription : ISpotifyId
    {
        string Title { get; }
    }
    public interface ISpotifyId
    {
        string Uri { get; }
        string Id { get; }
        string ToHexId();
        string ToMercuryUri();
        SpotifyType Type { get; }
    }
}
