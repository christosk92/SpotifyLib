using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public partial class PathFinderAlbumTracks
    {
        [J("data")] public Data Data { get; set; }
    }

    public partial class Data
    {
        [J("album")] public Album Album { get; set; }
    }

    public partial class Album
    {
        [J("playability")] public Playability Playability { get; set; }
        [J("tracks")] public Tracks Tracks { get; set; }
    }

    public partial class Playability
    {
        [J("playable")] public bool Playable { get; set; }
    }

    public partial class Tracks
    {
        [J("totalCount")] public long TotalCount { get; set; }
        [J("items")] public List<TracksItem> Items { get; set; }
    }

    public partial class TracksItem
    {
        [J("uid")] public string Uid { get; set; }
        [J("track")] public Track Track { get; set; }
    }

    public partial class Track
    {
        [J("saved")] public bool Saved { get; set; }
        [J("uri")] public string Uri { get; set; }
        [J("name")] public string Name { get; set; }
        [J("playcount")] public string Playcount { get; set; }
        [J("discNumber")] public int DiscNumber { get; set; }
        [J("trackNumber")] public long TrackNumber { get; set; }
        [J("contentRating")] public ContentRating ContentRating { get; set; }
        [J("relinkingInformation")] public RelinkingInformation RelinkingInformation { get; set; }
        [J("duration")] public Duration Duration { get; set; }
        [J("playability")] public Playability Playability { get; set; }
        [J("artists")] public Artists Artists { get; set; }
    }

    public partial class Artists
    {
        [J("items")] public List<ArtistsItem> Items { get; set; }
    }

    public partial class ArtistsItem
    {
        [J("uri")] public string Uri { get; set; }
        [J("profile")] public Profile Profile { get; set; }
    }

    public partial class Profile
    {
        [J("name")] public string Name { get; set; }
    }

    public partial class ContentRating
    {
        [J("label")] public string Label { get; set; }
    }

    public partial class Duration
    {
        [J("totalMilliseconds")] public int TotalMilliseconds { get; set; }
    }

    public partial class RelinkingInformation
    {
        [J("uri")] public string Uri { get; set; }
        [J("isRelinked")] public bool IsRelinked { get; set; }
    }
}
