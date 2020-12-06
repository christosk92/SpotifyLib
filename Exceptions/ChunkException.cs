using System;
using JetBrains.Annotations;

namespace SpotifyLib.Exceptions
{
    public class ChunkException : Exception
    {
        public ChunkException([NotNull] Exception cause) : base(cause.Message)
        {
        }

        protected ChunkException()
        {
        }

        public static ChunkException FromStreamError(short streamError)
        {
            return new ChunkException(new Exception("Failed due to stream error, code: " + streamError));
        }
    }
}