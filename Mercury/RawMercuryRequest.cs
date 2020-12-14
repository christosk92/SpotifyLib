using System;
using System.Collections.Generic;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify;

namespace SpotifyLib.Mercury
{
    public class RawMercuryRequest
    {
        public readonly Header _header;
        public readonly List<byte[]> _payload;


        public static RawMercuryRequest Get(string uri)
        {
            return new RawMercuryRequest(uri, "GET");
        }
        public static RawMercuryRequest Sub(string uri)
        {
            var r = new RawMercuryRequest(uri, "SUB");
            r.AddUserField("Accept-Language", "1202656e");
            return r;
        }
        public static RawMercuryRequest Unsub(string uri)
        {
            return new RawMercuryRequest(uri, "UNSUB");
        }

        public RawMercuryRequest(string uri,
            string method,
            IEnumerable<MercuryRequest> multiRequests = null)
        {
            _payload = new List<byte[]>();
            _header = new Header
            {
                Method = method,
                Uri = uri
            };
            if (multiRequests == null) return;
            _header.ContentType = "vnd.spotify/mercury-mget-request";
            var multi = new MercuryMultiGetRequest();
            multi.Request.AddRange(multiRequests);
            _payload.Add(multi.ToByteArray());
        }

        public RawMercuryRequest ContentType(string contentType)
        {
            _header.ContentType = contentType;
            return this;
        }
        public void AddUserField([NotNull] String key, [NotNull] String value)
        {
            AddUserField(new UserField
            {
                Key = key,
                Value = ByteString.CopyFromUtf8(value)
            });
        }
        public void AddUserField(UserField field)
            => this._header.UserFields.Add(field);
    }
}
