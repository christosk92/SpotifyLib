using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SpotifyLib.Models
{
    public class GenericSpotifyItem
    {
        private string _uri;

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri
        {
            get => _uri;
            set
            {
                _uri = value;
                Id ??= value.Split(':').LastOrDefault();
            }
        }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id
        {
            get;
            set;
        }
    }
}
