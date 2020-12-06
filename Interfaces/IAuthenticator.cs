using System.Threading.Tasks;
using Spotify;

namespace SpotifyLib.Interfaces
{
    public interface IAuthenticator
    {
        Task<LoginCredentials> Get();
    }
}

