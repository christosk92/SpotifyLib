using System;
using System.Text;
using System.Threading;
using System.Web;
using GuardAgainstLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLib.Helpers;

namespace SpotifyLib.Mercury
{
    public enum SearchType
    {
        Quick,
        Overview,
        Full
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

        public JObject Request(
            [NotNull] SearchRequest req)
        {
            if (req.Username.IsEmpty()) req.Username = _session.Username;
            if (req.Country.IsEmpty()) req.Country = _session.CountryCode;
            if (req.Locale.IsEmpty()) req.Locale = _session.Locale;

            var mercuryResponse = _session.Mercury().SendSync(new JsonMercuryRequest<string>(RawMercuryRequest.Get(req.BuildUrl())));
            return JObject.Parse(mercuryResponse);
        }
    }

    public struct SearchRequest
    {
        private static int Sequence;
        private readonly string _query;
        public int Limit { get; set; }
        public string ImageSize { get; set; }
        public string Catalogue { get; set; }
        public string Country { get; set; }
        public string Locale { get; set; }
        public string Username { get; set; }
        public SearchType SearchType { get; }
        public SearchRequest(
            SearchType searchType,
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
            SearchType = searchType;
            ImageSize = imageSize;
            Catalogue = catalogue;
            Country = country;
            Locale = locale;
            Username = name;
            GuardAgainst.ArgumentBeingNullOrEmpty(query);
        }

        public string BuildUrl()
        {
            switch (SearchType)
            {
                case SearchType.Overview:
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
                case SearchType.Quick:
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