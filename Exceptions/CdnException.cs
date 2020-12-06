using System;
using JetBrains.Annotations;

namespace SpotifyLib.Exceptions
{

    public class CdnException : Exception
    {

        public CdnException([NotNull] string message) : base(message)
        {
        }

        public CdnException([NotNull] Exception ex) : base(ex.Message, ex)
        {

        }
    }
}