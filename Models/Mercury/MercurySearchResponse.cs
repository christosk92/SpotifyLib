using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyLib.Enums;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using N = Newtonsoft.Json.NullValueHandling;
namespace SpotifyLib.Models.Mercury
{
    public enum BaseType 
    {
        Track,
        Artist,
        Album,
        Episode,
        Show,
        Profile,
        Playlist,
        Station,
        Recommendation,
        Unknown,
        Genre
    }
    public class GenericHit : GenericSpotifyItem
    {
        public BaseType Type2 { get; set; }
    }

    public partial class MercurySearchResponse
    {
        [J("results")] public Results Results { get; set; }
        [J("requestId")] public string RequestId { get; set; }
        [J("categoriesOrder")] public List<string> CategoriesOrder { get; set; }
    }

    public partial class Results
    {
        [J("tracks")] public Tracks Tracks { get; set; }
        [J("albums")] public Albums Albums { get; set; }
        [J("artists")] public Artists Artists { get; set; }
        [J("playlists")] public Playlists Playlists { get; set; }
        [J("profiles")] public Profiles Profiles { get; set; }
        [J("genres")] public Genres Genres { get; set; }
        [J("topHit")] public TopHits TopHit { get; set; }
        [J("shows")] public Shows Shows { get; set; }
        [J("audioepisodes")] public Audioepisodes Audioepisodes { get; set; }
        [J("topRecommendations")] public TopRecommendations TopRecommendations { get; set; }
    }

    public partial class Albums
    {
        [J("hits")] public List<AlbumsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class AlbumsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("artists")] public List<Album> Artists { get; set; }
        [JsonIgnore]
        public string ArtissString => string.Join(",",
            Artists?.Select(z => z.Name) ?? Array.Empty<string>());
    }

    public partial class Album
    {
        [J("name")] public string Name { get; set; }
        [J("uri")] public string Uri { get; set; }
    }
    public partial class TopHits
    {
        [J("hits")] public List<TopHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }


    public partial class Artists
    {
        [J("hits")] public List<ArtistsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class TopHit : GenericHit
    {
        public TopHit() : base()
        {

        }
        public static CancellationTokenSource cts;
        [JsonIgnore]
        public static string PreviousUrl;
        [JsonIgnore]
        public static string PreviousHeaderUrl;
        [J("name")] public string Name { get; set; }
        private bool IsUser => string.IsNullOrEmpty(Author);

        private string _uri;
        [J("uri")]
        public new string Uri
        {
            get => _uri;
            set
            {
                base.Id = value.Split(':').Last();
                _uri = value;
                if (value.StartsWith("spotify:artist"))
                {
                    FetchHeaderUrl = Task.Run(async () =>
                    {
                        if (PreviousUrl == value)
                            return PreviousHeaderUrl;
                        var r = await SpotifySession.Current.Api().Artist.GetArtist(value.Split(':').Last());
                        var x = r.Data.Artist?.Visuals?.HeaderImage?.Sources?.FirstOrDefault()?.Url;
                        PreviousHeaderUrl = x;
                        PreviousUrl = value;
                        return x;
                    }, cts.Token);
                }

                if (value.Contains("playlist"))
                {
                    var regexMatch = Regex.Match(value, "spotify:user:(.*):playlist:(.{22})");
                    if (regexMatch.Success)
                    {
                        Type2 = BaseType.Playlist;
                        return;
                    }
                    else
                    {
                        regexMatch = Regex.Match(value, "spotify:playlist:(.{22})");
                        if (regexMatch.Success)
                        {
                            Type2 = BaseType.Playlist;
                            return;
                        }
                    }
                }
                Type2 = Uri.Split(':')[1] switch
                {
                    null => throw new ArgumentNullException(nameof(Uri)),
                    "" => throw new ArgumentException($"{nameof(Uri)} cannot be empty", nameof(Uri)),
                    "track" => BaseType.Track,
                    "artist" => BaseType.Artist,
                    "album" => BaseType.Album,
                    "episode" => BaseType.Episode,
                    "show" => BaseType.Show,
                    "playlist" => BaseType.Playlist,
                    "user" => BaseType.Profile,
                    "app" => BaseType.Genre,
                    _ => BaseType.Unknown
                };
            }
        }

        [JsonIgnore]
        public Task<string> FetchHeaderUrl;
        public string Type { get; set; }
        [J("image")] public string Image { get; set; }
        [J("artists", NullValueHandling = NullValueHandling.Ignore)] public List<Album> Artists { get; set; }
        [J("album", NullValueHandling = NullValueHandling.Ignore)] public Album Album { get; set; }
        [J("duration", NullValueHandling = NullValueHandling.Ignore)] public long Duration { get; set; }
        [J("mogef19", NullValueHandling = NullValueHandling.Ignore)] public bool Mogef19 { get; set; }
        [J("popularity", NullValueHandling = NullValueHandling.Ignore)] public long Popularity { get; set; }
        [J("lyricsMatch", NullValueHandling = NullValueHandling.Ignore)] public bool LyricsMatch { get; set; }
        [J("followersCount", NullValueHandling = NullValueHandling.Ignore)] public int followersCount { get; set; }
        [J("author", NullValueHandling = NullValueHandling.Ignore)] public string Author { get; set; }
    }

    public partial class ArtistsHit : GenericHit
    {
        private string _image;
        private string _name;
        [J("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                var initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value, "$1");
            }
        }

        [J("image")]
        public string Image
        {
            get => _image ?? "ms-appx:///Assets/DefaultIcon.png";
            set
            {
                _image = value;
            }
        }
        [JsonIgnore]
        public string Initials
        {
            get;
            set;
        }
    }

    public partial class Audioepisodes
    {
        [J("hits")] public List<AudioepisodesHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class AudioepisodesHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("explicit")] public bool Explicit { get; set; }
        [J("duration")] public long Duration { get; set; }
        [J("popularity")] public long Popularity { get; set; }
        [J("musicAndTalk")] public bool MusicAndTalk { get; set; }
    }

    public partial class Genres
    {
        [J("hits")] public List<object> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class Playlists
    {
        [J("hits")] public List<PlaylistsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class PlaylistsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("followersCount")] public long FollowersCount { get; set; }
        [J("author")] public string Author { get; set; }
    }

    public partial class Profiles
    {
        [J("hits")] public List<ProfilesHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class ProfilesHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("followersCount")] public long FollowersCount { get; set; }
        [J("image", NullValueHandling = N.Ignore)] public Uri Image { get; set; }
    }

    public partial class Shows
    {
        [J("hits")] public List<ShowsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class ShowsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("showType")] public string ShowType { get; set; }
        [J("popularity")] public long Popularity { get; set; }
        [J("musicAndTalk")] public bool MusicAndTalk { get; set; }
    }

    public partial class TopRecommendations
    {
        [J("hits")] public List<TopRecommendationsHit> Hits { get; set; }
        [J("title")] public string Title { get; set; }
    }

    public partial class TopRecommendationsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("followersCount")] public int FollowersCount { get; set; }
        [J("author")] public string Author { get; set; }
    }

    public partial class Tracks
    {
        [J("hits")] public List<TracksHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class TracksHit : GenericHit
    {
        [JsonIgnore]
        public string ArtissString => string.Join(",",
        Artists?.Select(z => z.Name) ?? Array.Empty<string>());
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("artists")] public List<Album> Artists { get; set; }
        [J("album")] public Album Album { get; set; }
        [J("duration")] public long Duration { get; set; }
        [J("mogef19")] public bool Mogef19 { get; set; }
        [J("popularity")] public long Popularity { get; set; }
        [J("lyricsMatch")] public bool LyricsMatch { get; set; }
    }
}