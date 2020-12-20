using Refit;

namespace SpotifyLib.Models.Api.Requests
{
    public class RecentlyPlayedRequest
    {
#nullable enable
        [AliasAs("country")] public string? Country { get; set; }

        [AliasAs("locale")] public string? Locale { get; set; }

        [AliasAs("market")] public string? market { get; set; }

        [AliasAs("timestamp")] public string? timestamp { get; set; }

        [AliasAs("platform")] public string? platform { get; set; }

        [AliasAs("content_limit")] public int? content_limit { get; set; }

        [AliasAs("limit")] public int? limit { get; set; }
        [AliasAs("offset")] public int? offset { get; set; } = 0;

        [AliasAs("types")] public string? types { get; set; }

        [AliasAs("image_style")] public string? image_style { get; set; }

#nullable disable
    }
}