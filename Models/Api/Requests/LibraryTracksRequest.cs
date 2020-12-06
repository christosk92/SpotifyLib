using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class LibraryTracksRequest
    {
        /// <summary>
        /// The maximum number of objects to return. Default: 20. Minimum: 1. Maximum: 50.
        /// </summary>
        /// <value></value>
        [AliasAs("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// The index of the first object to return.
        /// Default: 0 (i.e., the first object). Use with limit to get the next set of objects.
        /// </summary>
        /// <value></value>
        [AliasAs("offset")]
        public int? Offset { get; set; }
    }
}