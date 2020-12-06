using System.Collections.Generic;
using JetBrains.Annotations;
using Spotify;

namespace SpotifyLib.Mercury
{
    public class MercuryRequests
    {
        private static readonly string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";

        private MercuryRequests()
        {
        }
        public static JsonMercuryRequest<string> ResolveContext([NotNull] string uri)
        {
            return new JsonMercuryRequest<string>(RawMercuryRequest.Get(
                $"hm://context-resolve/v1/{uri}"));
        }
        public static JsonMercuryRequest<StoredToken> RequestToken([NotNull] string deviceId, [NotNull] string[] scope)
        {
            return new JsonMercuryRequest<StoredToken>(RawMercuryRequest.Get(
                $"hm://keymaster/token/authenticated?scope={string.Join(",", scope)}&client_id={KEYMASTER_CLIENT_ID}&device_id={deviceId}"));
        }
        
        public static JsonMercuryRequest<string> GetGenericJson([NotNull] string uri)
        {
            return new JsonMercuryRequest<string>(RawMercuryRequest.Get(uri));
        }

        public static JsonMercuryRequest<MercuryArtist> GetArtist([NotNull] ArtistId id)
        {
            return new JsonMercuryRequest<MercuryArtist>(RawMercuryRequest.Get(id.ToMercuryUri()));
        }
        public static RawMercuryRequest AutoplayQuery([NotNull] string context)
        {
            return RawMercuryRequest.Get("hm://autoplay-enabled/query?uri=" + context);
        }
        public static JsonMercuryRequest<string> GetStationFor([NotNull] string context)
        {
            return new JsonMercuryRequest<string>(RawMercuryRequest.Get("hm://radio-apollo/v3/stations/" + context));
        }
        public static ProtobuffedMercuryRequest<MercuryMultiGetReply> MultiGet(string uri, IEnumerable<MercuryRequest> requests)
        {
            var request = new RawMercuryRequest(
                    uri,
                    "GET",
                    requests)
                .ContentType("vnd.spotify/mercury-mget-request");
            return new ProtobuffedMercuryRequest<MercuryMultiGetReply>(request, MercuryMultiGetReply.Parser);
        }
    }
}
