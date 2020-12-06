using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class PlaylistTracksRequest
    {
#nullable enable
        [AliasAs("additional_types")] public string? additional_types { get; set; }

        [AliasAs("limit")] public int? limit { get; set; }

        [AliasAs("market")] public string? market { get; set; }

        [AliasAs("offset")] public int? offset { get; set; }  

#nullable disable
    }
}