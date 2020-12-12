using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using GuardAgainstLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLib.Helpers;

namespace SpotifyLib.Mercury
{
    public class SearchBase
    {
    }

    public class QuickSearch : SearchBase
    {
        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        public partial class Section
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("items")]
            public List<SearchItem> Items { get; set; }

            public partial class SearchItem
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("uri")]
                public string Uri { get; set; }

                [JsonProperty("image")]
                public Uri Image { get; set; }

                [JsonProperty("album", NullValueHandling = NullValueHandling.Ignore)]
                public SearchAlbum Album { get; set; }

                [JsonProperty("artists", NullValueHandling = NullValueHandling.Ignore)]
                public List<SearchAlbum> Artists { get; set; }

                [JsonProperty("fromLyrics", NullValueHandling = NullValueHandling.Ignore)]
                public bool? FromLyrics { get; set; }

                [JsonProperty("explicit", NullValueHandling = NullValueHandling.Ignore)]
                public bool? Explicit { get; set; }

                [JsonProperty("followers", NullValueHandling = NullValueHandling.Ignore)]
                public long? Followers { get; set; }

                public partial class SearchAlbum
                {
                    [JsonProperty("name")]
                    public string Name { get; set; }

                    [JsonProperty("uri")]
                    public string Uri { get; set; }
                }
            }
        }
    }
    public class FullSearch : SearchBase
    {

    }
    public readonly struct MercurySearchManager
    {
        internal static readonly string MainSearch = "hm://searchview/km/v4/search/";
        internal static readonly string QuickSearch = "hm://searchview/km/v3/suggest/";
        private readonly SpotifySession _session;

        public MercurySearchManager([NotNull] SpotifySession session)
        {
            this._session = session;
        }

        public T Request<T>(
            [NotNull] SearchRequest<T> req,
            bool outputString) where T : SearchBase
        {
            if (req.Username.IsEmpty()) req.Username = _session.Username;
            if (req.Country.IsEmpty()) req.Country = _session.CountryCode;
            if (req.Locale.IsEmpty()) req.Locale = _session.Locale;

            if (!outputString)
            {
                return _session.Mercury().SendSync(new JsonMercuryRequest<T>(RawMercuryRequest.Get(req.BuildUrl())));
            }
            else
            {
                var test = _session.Mercury().SendSync(new JsonMercuryRequest<string>(RawMercuryRequest.Get(req.BuildUrl())));
                Debug.WriteLine(test);
                return default(T);
            }
        }
    }

    public struct SearchRequest<T> where T : SearchBase
    {
        private static int Sequence;
        private readonly string _query;
        public int Limit { get; set; }
        public string ImageSize { get; set; }
        public string Catalogue { get; set; }
        public string Country { get; set; }
        public string Locale { get; set; }
        public string Username { get; set; }
        public SearchRequest(
            [NotNull] string query,
            string imageSize,
            string catalogue,
            string country,
            string locale,
            string name,
            int limit = 10)
        {
            this._query = query.Trim();
            Limit = limit;
            ImageSize = imageSize;
            Catalogue = catalogue;
            Country = country;
            Locale = locale;
            Username = name;
            GuardAgainst.ArgumentBeingNullOrEmpty(query);
        }

        public string BuildUrl()
        {
            switch (typeof(T))
            {
                case { } intType when intType == typeof(FullSearch):
                    var url =
                        Flurl.Url.Combine(MercurySearchManager.MainSearch,
                            HttpUtility.UrlEncode(_query, Encoding.UTF8));
                    url += "?entityVersion=2";
                    url += "&limit=" + Limit;
                    url += "&imageSize=" + HttpUtility.UrlEncode(ImageSize, Encoding.UTF8);
                    url += "&catalogue=" + HttpUtility.UrlEncode(Catalogue, Encoding.UTF8);
                    url += "&country=" + HttpUtility.UrlEncode(Country, Encoding.UTF8);
                    url += "&locale=" + HttpUtility.UrlEncode(Locale, Encoding.UTF8);
                    url += "&username=" + HttpUtility.UrlEncode(Username, Encoding.UTF8);
                    return url;
                case { } intType when intType == typeof(QuickSearch):
                    var quickUrl =
                        Flurl.Url.Combine(MercurySearchManager.QuickSearch,
                            HttpUtility.UrlEncode(_query, Encoding.UTF8));
                    quickUrl += "?limit=5";
                    quickUrl += "&intent=2516516747764520149";
                    quickUrl += "&sequence=" + Sequence;
                    quickUrl += "&catalogue=" + HttpUtility.UrlEncode(Catalogue, Encoding.UTF8);
                    quickUrl += "&country=" + HttpUtility.UrlEncode(Country, Encoding.UTF8);
                    quickUrl += "&locale=" + HttpUtility.UrlEncode(Locale, Encoding.UTF8);
                    quickUrl += "&username=" + HttpUtility.UrlEncode(Username, Encoding.UTF8);
                    Interlocked.Increment(ref Sequence);
                    return quickUrl;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}