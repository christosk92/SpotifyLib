using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public partial class PlaylistExtenderResult
    {
        [JsonProperty("recommendedTracks")]
        public List<RecommendedTrack> RecommendedTracks { get; set; }

        [JsonProperty("request")]
        public Request Request { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }
    }

    public partial class Details
    {
        [JsonProperty("aggregateTracksFromPlaylists")]
        public long AggregateTracksFromPlaylists { get; set; }

        [JsonProperty("playlistLoad")]
        public long PlaylistLoad { get; set; }

        [JsonProperty("prepopulateAndFilter")]
        public long PrepopulateAndFilter { get; set; }

        [JsonProperty("totalTime")]
        public long TotalTime { get; set; }
    }

    public partial class RecommendedTrack : GenericSpotifyItem
    {

        [JsonProperty("originalId")]
        public string OriginalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("album")]
        public Album Album { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("contentRating")]
        public List<object> ContentRating { get; set; }
    }

    public partial class Album
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("largeImageUrl")]
        public string LargeImageUrl { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }


    public partial class Request
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("playlistURI")]
        public string PlaylistUri { get; set; }

        [JsonProperty("trackIDs")]
        public List<string> TrackIDs { get; set; }

        [JsonProperty("artistIDs")]
        public List<object> ArtistIDs { get; set; }

        [JsonProperty("trackSkipIDs")]
        public List<object> TrackSkipIDs { get; set; }

        [JsonProperty("playlistSkipIDs")]
        public List<object> PlaylistSkipIDs { get; set; }

        [JsonProperty("artistSkipIDs")]
        public List<object> ArtistSkipIDs { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("numResults")]
        public long NumResults { get; set; }

        [JsonProperty("condensed")]
        public bool Condensed { get; set; }

        [JsonProperty("decoration")]
        public bool Decoration { get; set; }

        [JsonProperty("family")]
        public string Family { get; set; }

        [JsonProperty("familyList")]
        public List<string> FamilyList { get; set; }
    }
}