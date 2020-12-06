using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class PrivateUser
    {
        private string _displayName;

        public string Country { get; set; } = default!;

        [JsonProperty("display_name")]
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                var initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value, "$1");
            }
        }

        public string Email { get; set; } = default!;

        [JsonIgnore]
        public string Initials
        {
            get;
            set;
        }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public Followers Followers { get; set; } = default!;

        public string Href { get; set; } = default!;

        public string Id { get; set; } = default!;

        public List<Image> Images { get; set; } = default!;

        public string Product { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string Uri { get; set; } = default!;
    }
}

