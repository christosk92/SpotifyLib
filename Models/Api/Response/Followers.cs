using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
	public class Followers
	{
		public string Href { get; set; } = default!;

		[JsonProperty("total")]
		public int Total { get; set; }
	}
}

