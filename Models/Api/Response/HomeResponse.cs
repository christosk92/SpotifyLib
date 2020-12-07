using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class HomeResponseTrimmed
    {
        [JsonProperty("tag_line")]
        public string TagLine { get; set; }
	}
	public class HomeResponse
	{
		[JsonProperty("content")]
		public HomeResponseContent Content { get; set; }

		[JsonProperty("custom_fields")]
		public CustomFields CustomFields { get; set; }

		[JsonProperty("external_urls")]
		public object ExternalUrls { get; set; }

		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("images")]
		public List<FluffyImage> Images { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("rendering")]
		public string Rendering { get; set; }

		[JsonProperty("tag_line")] 
        public string TagLine { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}

	public partial class HomeResponseContent
	{
		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("items")]
		public List<PurpleItem> Items { get; set; }

		[JsonProperty("limit")]
		public long? Limit { get; set; }

		[JsonProperty("next")]
		public object Next { get; set; }

		[JsonProperty("offset")]
		public long? Offset { get; set; }

		[JsonProperty("previous")]
		public object Previous { get; set; }

		[JsonProperty("total")]
		public long? Total { get; set; }
	}

	public partial class PurpleItem
	{
		[JsonProperty("content")]
		public ItemContent Content { get; set; }

		[JsonProperty("custom_fields")]
		public CustomFields CustomFields { get; set; }

		[JsonProperty("external_urls")]
		public object ExternalUrls { get; set; }

		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("images")]
        public List<FluffyImage> Images { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("rendering")]
		public string Rendering { get; set; }

		[JsonProperty("tag_line")]
		public string TagLine { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}

	public partial class ItemContent
	{
		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("items")]
		public FluffyItem[] Items { get; set; }

		[JsonProperty("limit")]
		public long? Limit { get; set; }

		[JsonProperty("next")]
		public Uri Next { get; set; }

		[JsonProperty("offset")]
		public long? Offset { get; set; }

		[JsonProperty("previous")]
		public object Previous { get; set; }

		[JsonProperty("total")]
		public long? Total { get; set; }
	}

	public partial class FluffyItem : GenericSpotifyItem
    {
        private string _desc;

        [JsonProperty("collaborative")]
		public bool? Collaborative { get; set; }

        [JsonProperty("description")]
        public string Description
        {
            get => _desc;
            set
            {
                var mainDoc = new HtmlDocument();
                mainDoc.LoadHtml(value);
                _desc = mainDoc.DocumentNode.InnerText;
            }
        }

		[JsonProperty("external_urls")]
		public ExternalUrls ExternalUrls { get; set; }

		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("images")]
        public List<FluffyImage> Images { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("owner")]
		public Owner Owner { get; set; }

		[JsonProperty("primary_color")]
		public object PrimaryColor { get; set; }

		[JsonProperty("public")]
		public object Public { get; set; }

		[JsonProperty("snapshot_id")]
		public string SnapshotId { get; set; }

		[JsonProperty("tracks")]
		public Tracks Tracks { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }


		[JsonProperty("followers")]
		public Followers Followers { get; set; }

		[JsonProperty("genres")]
		public string[] Genres { get; set; }

		[JsonProperty("popularity")]
		public long? Popularity { get; set; }

		[JsonProperty("album_type")]
		public string AlbumType { get; set; }

		[JsonProperty("artists")]
		public Artist[] Artists { get; set; }

		[JsonProperty("release_date")]
		public string? ReleaseDate { get; set; }

		[JsonProperty("release_date_precision")]
		public string? ReleaseDatePrecision { get; set; }

		[JsonProperty("total_tracks")]
		public long? TotalTracks { get; set; }

		[JsonProperty("available_markets")]
		public string[] AvailableMarkets { get; set; }

		[JsonProperty("copyrights")]
		public object[] Copyrights { get; set; }

		[JsonProperty("explicit")]
		public bool? Explicit { get; set; }

		[JsonProperty("is_externally_hosted")]
		public bool? IsExternallyHosted { get; set; }

		[JsonProperty("languages")]
		public string[] Languages { get; set; }

		[JsonProperty("media_type")]
		public string MediaType { get; set; }

		[JsonProperty("publisher")]
		public string Publisher { get; set; }

		[JsonProperty("total_episodes")]
		public long? TotalEpisodes { get; set; }
    }

	public partial class Artist : GenericSpotifyItem
	{
		[JsonProperty("external_urls")]
		public ExternalUrls ExternalUrls { get; set; }

		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("uri")]
		public string Uri { get; set; }
	}

	public partial class ExternalUrls
	{
		[JsonProperty("spotify")]
		public Uri Spotify { get; set; }
	}

	public partial class Owner
	{
		[JsonProperty("display_name")]
		public string DisplayName { get; set; }

		[JsonProperty("external_urls")]
		public ExternalUrls ExternalUrls { get; set; }

		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("uri")]
		public string Uri { get; set; }
	}

	public partial class Tracks
	{
		[JsonProperty("href")]
		public Uri Href { get; set; }

		[JsonProperty("total")]
		public long? Total { get; set; }
	}

	public partial class CustomFields
	{
	}

    public partial class FluffyImage
    {
        [JsonProperty("height")]
        public long? Height { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }
    }
}
