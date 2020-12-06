using System;
using System.Diagnostics;

namespace SpotifyLib.Exceptions
{
    public class MercuryException : Exception
    {
        public MercuryException(MercuryResponse response) : base(response.StatusCode.ToString())
        {
            Debug.WriteLine("Mercury failed: Response " + response.StatusCode);
        }
    }
}