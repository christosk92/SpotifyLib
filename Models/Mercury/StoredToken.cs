using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpotifyLib.Services;

namespace SpotifyLib.Models.Mercury
{
    public class StoredToken
    {
        [JsonProperty("accessToken")] public string AccessToken { get; set; }
        [JsonProperty("expiresIn")] public int ExpiresIn { get; set; }
        [JsonProperty("tokenType")] public string TokenType { get; set; }
        [JsonProperty("scope")] public string[] Scope { get; set; }

        private readonly long _timeStamp;
        private const int TOKEN_EXPIRE_THRESHOLD = 10;

        public StoredToken()
        {
            _timeStamp = TimeProvider.CurrentTimeMillis();
        }

        public bool Expired() =>
            _timeStamp + (ExpiresIn - TOKEN_EXPIRE_THRESHOLD) * 1000 < TimeProvider.CurrentTimeMillis();

        public override string ToString()
        {
            return "StoredToken{" +
                   "expiresIn=" + ExpiresIn +
                   ", accessToken='" + AccessToken +
                   "', scopes=" + string.Join(",", Scope) +
                   ", timestamp=" + _timeStamp +
                   '}';
        }

        public bool HasScope([NotNull] string scope) => Scope.ToList().Exists(x => x == scope);

        public bool HasScopes(string[] sc) =>
            Scope.OrderBy(kvp => kvp)
                .SequenceEqual(sc.OrderBy(kvp => kvp));
    }
}