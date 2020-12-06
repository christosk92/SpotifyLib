using System.Collections.Generic;
using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class TracksRequest 
    {
        /// <summary>
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </summary>
        [AliasAs("ids")]
        public IList<string> Ids { get; }

#nullable enable
        /// <summary>
        /// An ISO 3166-1 alpha-2 country code or the string from_token.
        /// Provide this parameter if you want to apply Track Relinking.
        /// </summary>
        [AliasAs("market")]
        public string? Market { get; set; }
#nullable disable


        /// <summary>
        /// </summary>
        /// <param name="ids">
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </param>
        public TracksRequest(
            IList<string> ids)
        {
            Ensure.ArgumentNotNullOrEmptyList(ids, nameof(ids));

            Ids = ids;
        }
    }
}
