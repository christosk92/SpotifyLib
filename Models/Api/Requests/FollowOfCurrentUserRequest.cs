using Refit;
using SpotifyLib.Attributes;

namespace SpotifyLib.Models.Api.Requests
{
    public class FollowOfCurrentUserRequest 
    {
        /// <summary>
        /// The ID type: currently only artist is supported.
        /// </summary>
        /// <param name="type"></param>
        public FollowOfCurrentUserRequest(Type type = Type.Artist)
        {
            TypeParam = type;
        }

        /// <summary>
        /// The ID type: currently only artist is supported.
        /// </summary>
        /// <value></value>
        [AliasAs("type")]
        public Type TypeParam { get; set; }

        /// <summary>
        /// The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.
        /// </summary>
        /// <value></value>
        [AliasAs("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// The last artist ID retrieved from the previous request.
        /// </summary>
        /// <value></value>
        [AliasAs("after")]
        public string? After { get; set; }

        public enum Type
        {
            [String("artist")]
            Artist
        }
    }
}