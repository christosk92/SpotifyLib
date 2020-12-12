using System;
using System.Text;
using System.Web;
using GuardAgainstLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLib.Helpers;

namespace SpotifyLib.Mercury
{
    public readonly struct MercurySearchManager
    {
        internal static readonly string BaseUrl = "hm://searchview/km/v4/search/";

        private readonly SpotifySession _session;

        public MercurySearchManager([NotNull] SpotifySession session)
        {
            this._session = session;
        }

        public JObject Request([NotNull] SearchRequest req)
        {
            if (req.Username.IsEmpty()) req.Username = _session.Username;
            if (req.Country.IsEmpty()) req.Country = _session.CountryCode;
            if (req.Locale.IsEmpty()) req.Locale = _session.Locale;

            var mercuryResponse = _session.Mercury().SendSync(new JsonMercuryRequest<string>(RawMercuryRequest.Get(req.BuildUrl())));
            return JObject.Parse(mercuryResponse);
        }
    }

    public class SearchRequest
    {
        private readonly string _query;
        public int Limit { get; set; } = 10;
        public string ImageSize { get; set; }
        public string Catalogue { get; set; }
        public string Country { get; set; }
        public string Locale { get; set; }
        public string Username { get; set; }

        public SearchRequest([NotNull] string query)
        {
            this._query = query.Trim();
            GuardAgainst.ArgumentBeingNullOrEmpty(query);
        }

        public string BuildUrl()
        {
            var url =
                Flurl.Url.Combine(MercurySearchManager.BaseUrl,
                    HttpUtility.UrlEncode(_query, Encoding.UTF8));

            url += "?entityVersion=2";
            url += "&limit=" + Limit;
            url += "&imageSize=" + HttpUtility.UrlEncode(ImageSize, Encoding.UTF8);
            url += "&catalogue=" + HttpUtility.UrlEncode(Catalogue, Encoding.UTF8);
            url += "&country=" + HttpUtility.UrlEncode(Country, Encoding.UTF8);
            url += "&locale=" + HttpUtility.UrlEncode(Locale, Encoding.UTF8);
            url += "&username=" + HttpUtility.UrlEncode(Username, Encoding.UTF8);
            return url;
        }
    }
}