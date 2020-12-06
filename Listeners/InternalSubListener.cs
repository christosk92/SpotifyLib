using JetBrains.Annotations;
using SpotifyLib.Interfaces;
using SpotifyLib.Mercury;

namespace SpotifyLib.Listeners
{
    internal class InternalSubListener
    {
        private readonly string _uri;
        private readonly ISubListener _listener;
        private readonly bool _isSub;

        internal InternalSubListener([NotNull] string uri,
            [NotNull] ISubListener listener,
            bool isSub)
        {
            this._uri = uri;
            this._listener = listener;
            this._isSub = isSub;
        }

        internal bool Matches(string uri)
        {
            return uri.StartsWith(this._uri);
        }
        internal void Dispatch([NotNull] MercuryResponse resp)
        {
            _listener.OnEvent(resp);
        }
    }
}
