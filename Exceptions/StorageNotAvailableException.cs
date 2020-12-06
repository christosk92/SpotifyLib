using JetBrains.Annotations;

namespace SpotifyLib.Exceptions
{
    public class StorageNotAvailableException : ChunkException
    {
        public readonly string cdnUrl;
        public StorageNotAvailableException([NotNull] string cdnUrl)
        {
            this.cdnUrl = cdnUrl;
        }
    }
}
