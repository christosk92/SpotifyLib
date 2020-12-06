using System.Linq;
using JetBrains.Annotations;
using Spotify;

namespace SpotifyLib.Mercury
{
    public class MercuryResponse
    {
        public readonly string Uri;
        public readonly BytesArrayList Payload;
        public readonly int StatusCode;

        public MercuryResponse(
            [NotNull] Header header,
            [NotNull] BytesArrayList payload)
        {
            this.Uri = header.Uri;
            this.StatusCode = header.StatusCode;
            this.Payload = payload.CopyOfRange(1, payload.Count());
        }
    }
}