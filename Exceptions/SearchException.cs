using System;

namespace SpotifyLib.Exceptions
{
    public class SearchException : Exception
    {
        public SearchException(int statusCode) : base($"Search failed with code {statusCode}")
        {
        }
    }
}