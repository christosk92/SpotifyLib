using J = Newtonsoft.Json.JsonPropertyAttribute;


namespace SpotifyLib.Models.Api.Requests
{
    public partial class RemoteRequest
    {
        [J("command")] public Command Command { get; set; }
    }

    public partial class Command
    {
        [J("context")] public Context Context { get; set; }
        [J("play_origin")] public PlayOrigin PlayOrigin { get; set; }
        [J("options")] public Options Options { get; set; }
        [J("endpoint")] public string Endpoint { get; set; }
    }

    public partial class Context
    {
        [J("uri")] public string Uri { get; set; }
        [J("url")] public string Url { get; set; }
        [J("metadata")] public Metadata Metadata { get; set; }
    }

    public partial class Metadata
    {
    }

    public partial class Options
    {
        [J("license")] public string License { get; set; }
        [J("skip_to")] public SkipTo SkipTo { get; set; }
        [J("player_options_override")] public PlayerOptionsOverride PlayerOptionsOverride { get; set; }
    }

    public partial class PlayerOptionsOverride
    {
        [J("repeating_track")] public bool RepeatingTrack { get; set; }
        [J("repeating_context")] public bool RepeatingContext { get; set; }
    }

    public partial class SkipTo
    {
        [J("track_index")] public long TrackIndex { get; set; }
    }

    public partial class PlayOrigin
    {
        [J("feature_identifier")] public string FeatureIdentifier { get; set; }
        [J("feature_version")] public string FeatureVersion { get; set; }
    }
}
