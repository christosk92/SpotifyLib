using System.Collections.Generic;
using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class LibraryRemoveAlbumsRequest
    {
        /// <summary>
        /// A comma-separated list of the Spotify IDs.
        /// For example: ids=4iV5W9uYEdYUVa79Axb7Rh,1301WleyT98MSxVHPZCA6M. Maximum: 50 IDs.
        /// </summary>
        [AliasAs("ids")]
        public IList<string> Ids { get; }
	}
}
