using System;
using Spotify;

namespace SpotifyLib.Exceptions
{
    public class SpotifyAuthenticatedException : Exception
    {
        public SpotifyAuthenticatedException(APLoginFailed loginFailed) : base(loginFailed.ErrorCode.ToString())
        {
            LogManager.Log(loginFailed.ErrorDescription);
        }
    }
}
