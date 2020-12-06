using System;
using System.Diagnostics;
using SpotifyLib.Mercury;

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