using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SpotifyLib.Mercury;
using SpotifyLib.Models.Mercury;

namespace SpotifyLib.Services
{
    public class TokenProvider
    {
        private static readonly int TOKEN_EXPIRE_THRESHOLD = 10;
        private readonly SpotifySession _session;
        private readonly List<StoredToken> tokens = new List<StoredToken>();

        public TokenProvider(SpotifySession session)
        {
            _session = session;
        }

        private StoredToken FindTokenWithAllScopes(string[] scopes)
        {
            return tokens.FirstOrDefault(token => token.HasScopes(scopes));
        }

        public StoredToken GetToken(params string[] scopes) {
            if (scopes.Length == 0) throw new ArgumentOutOfRangeException(
                nameof(scopes), 
                "provide atleast 1 scope");

            var token = FindTokenWithAllScopes(scopes);
            if (token != null) {
                if (token.Expired()) tokens.Remove(token);
                else return token;
            }

            Debug.WriteLine(
                $"Token expired or not suitable, requesting again. scopes: {string.Join(",", scopes)}, oldToken: {token}");

            token = _session.Mercury().SendSync(MercuryRequests.RequestToken(_session.DeviceId, scopes));

            Debug.WriteLine($"Updated token successfully! scopes: {string.Join(",", scopes)}, newToken: {token}");
            tokens.Add(token);
            return token;
        }
    }
}
