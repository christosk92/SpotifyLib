using System;

namespace SpotifyLib.Exceptions
{
    public class AesKeyException : Exception
    {
        public AesKeyException(string message) : base(message)
        {

        }
    }
}
