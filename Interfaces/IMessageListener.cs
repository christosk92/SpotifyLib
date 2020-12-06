using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpotifyLib.Interfaces
{
    public interface IMessageListener
    {
        Task OnMessage([NotNull] string uri, [NotNull] Dictionary<string, string> headers, [NotNull] byte[] payload);
    }
}
