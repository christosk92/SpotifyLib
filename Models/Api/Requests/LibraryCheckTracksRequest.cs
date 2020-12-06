using System.Collections.Generic;
using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class LibraryCheckTracksRequest
    {
        /// <summary>
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </summary>
        [AliasAs("ids")]
        public IList<string> Ids { get; }


        /// <summary>
        /// </summary>
        /// <param name="ids">
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </param>
        public LibraryCheckTracksRequest(
            IList<string> ids)
        {
            Ensure.ArgumentNotNull(ids, nameof(ids));

            Ids = ids;
        }
    }
}

