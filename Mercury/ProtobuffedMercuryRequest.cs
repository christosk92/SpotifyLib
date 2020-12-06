using System.Linq;
using Google.Protobuf;
using JetBrains.Annotations;

namespace SpotifyLib.Mercury
{
    public class ProtobuffedMercuryRequest<T> where T : IMessage<T>
    {
        private readonly MessageParser<T> _parser;
        public readonly RawMercuryRequest Request;

        public ProtobuffedMercuryRequest([NotNull] RawMercuryRequest request, MessageParser<T> parser)
        {
            this.Request = request;
            _parser = parser;
        }
        public static byte[] Combine(byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
        public T Instantiate(MercuryResponse read) => _parser.ParseFrom(Combine(read.Payload.ToArray()));
    }
}
