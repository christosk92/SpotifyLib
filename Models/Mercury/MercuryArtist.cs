using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Mercury
{
    public class ArtistHeaderedItem
    {

    }
    public partial class MercuryArtist : GenericSpotifyItem
    {
        private TopTracks _topTracks;
        private PinnedItem _pinnedItem;
        private Info _info;
        [JsonIgnore] public string Initials { get; set; }

        [JsonProperty("info")]
        public Info Info
        {
            get => _info;
            set
            {
                _info = value;
                Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value.Name, "$1");
                if (PinnedItem != null)
                {
                    PinnedItem.ArtistInfo = value;
                }
            }
        }

        [JsonProperty("header_image")]
        public HeaderImage HeaderImage { get; set; }

        [JsonProperty("top_tracks")]
        public TopTracks TopTracks
        {
            get => _topTracks;
            set
            {
               // value.Tracks.ForEach(k => k.ContextUri = Uri);
               // var j = value.Tracks.Select((k, i) => (k, i))
                //    .ToList();
               // j.ForEach(k => k.k.ContextIndex = k.i + 1);
                //value.Tracks = j.Select(z => z.k).ToList();
                _topTracks = value;
            }
        }

        [JsonProperty("upcoming_concerts")]
        public UpcomingConcerts UpcomingConcerts { get; set; }

        [JsonProperty("related_artists")]
        public RelatedArtists RelatedArtists { get; set; }

        [JsonProperty("biography")]
        public Biography Biography { get; set; }

        [JsonProperty("releases")]
        public Releases Releases { get; set; }

        [JsonProperty("merch")]
        public Merch Merch { get; set; }

        [JsonProperty("gallery")]
        public Gallery Gallery { get; set; }

        [JsonProperty("latest_release")]
        public LatestReleaseElement LatestRelease { get; set; }

        [JsonProperty("published_playlists")]
        public PublishedPlaylists PublishedPlaylists { get; set; }

        [JsonProperty("monthly_listeners")]
        public MonthlyListeners MonthlyListeners { get; set; }

        [JsonProperty("creator_about")]
        public CreatorAbout CreatorAbout { get; set; }

        [JsonProperty("pinned_item")]
        public PinnedItem PinnedItem
        {
            get => _pinnedItem;
            set
            {
                if (Info?.Portraits != null)
                {
                    value.ArtistInfo = Info;
                }
                _pinnedItem = value;
            }
        }
    }

    public partial class Biography
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class CreatorAbout
    {
        [JsonProperty("monthlyListeners")]
        public long? MonthlyListeners { get; set; }

        [JsonProperty("globalChartPosition")]
        public long? GlobalChartPosition { get; set; }
    }

    public partial class Gallery
    {
        [JsonProperty("images")]
        public List<Cover> Images { get; set; }
    }


    public partial class HeaderImage
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("offset")]
        public long? Offset { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("portraits")]
        public List<Cover> Portraits { get; set; }

        [JsonProperty("verified")]
        public bool? Verified { get; set; }
    }

    public partial class LatestReleaseElement : ArtistHeaderedItem
    {
        public static string GetAbbreviatedFromFullName(int year, int month, int day)
        {
            string monthName = new DateTime(year, month, day)
                .ToString("MMM", CultureInfo.InvariantCulture);
            return monthName.ToUpper();
        }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public int TrackCount { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonIgnore] public string Description => $"{GetAbbreviatedFromFullName(Year, Month, Day)} {Day}, {Year}";
    }

    public partial class Merch
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("image_uri")]
        public Uri ImageUri { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("uuid")]
        public long? Uuid { get; set; }
    }

    public partial class MonthlyListeners
    {
        [JsonProperty("listener_count")]
        public long? ListenerCount { get; set; }
    }

    public partial class PinnedItem : ArtistHeaderedItem
    {
        private string _sub;
        private Info _info;

        [JsonIgnore]
        public Info ArtistInfo
        {
            get => _info;
            set
            {
                _info = value;
                PostedByString = $"Posted by {value.Name}";
            }
        }
        [JsonIgnore]
        public string PostedByString { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle
        {
            get => _sub;
            set
            {
                _sub = value;
                if (value != null)
                {
                    PostedByString = value;
                }
            }
        }

        [JsonProperty("secondsToExpiration")]
        public long? SecondsToExpiration { get; set; }
    }

    public partial class PublishedPlaylists
    {
        [JsonProperty("playlists")]
        public List<Playlist> Playlists { get; set; }
    }

    public partial class Playlist : DiscographyItem
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("follower_count")]
        public long FollowerCount { get; set; }
    }

    public partial class RelatedArtists
    {
        [JsonProperty("artists")]
        public List<RelatedArtistsArtist> Artists { get; set; }
    }

    public partial class RelatedArtistsArtist
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        private string _name;
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value, "$1");
            }
        }
        [JsonIgnore] public string Initials { get; set; }
        [JsonProperty("portraits")]
        public List<Cover> Portraits { get; set; }
    }

    public partial class Releases
    {
        [JsonProperty("albums")]
        public Albums Albums { get; set; }

        [JsonProperty("singles")]
        public Albums Singles { get; set; }

        [JsonProperty("appears_on")]
        public AppearsOn AppearsOn { get; set; }

        [JsonProperty("compilations")]
        public Compilations Compilations { get; set; }
    }

    public partial class Albums
    {
        [JsonProperty("releases")]
        public List<AlbumsRelease> Releases { get; set; }

        [JsonProperty("total_count")]
        public long? TotalCount { get; set; }
    }

    public class DiscographyItem : GenericSpotifyItem
    {
        [JsonIgnore]
        public string DerivedFrom { get; set; }
    }
    public partial class AlbumsRelease : DiscographyItem
    {
        private List<Disc> _discs;
        private List<DiscTrack> _tracks;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("year")]
        public long? Year { get; set; }

        [JsonProperty("track_count")]
        public long? TrackCount { get; set; }

        public delegate void InvokeDelegate(AlbumsRelease sender,
            List<DiscTrack> obj);
        public static InvokeDelegate InvokeMethod;

        [JsonProperty("discs",
            DefaultValueHandling = DefaultValueHandling.Include)]
        public List<Disc> Discs
        {
            get => _discs;
            set
            {
                _discs = value;
                Tracks = value.SelectMany(z => z.Tracks).ToList();
            }
        }

        public async Task FetchTracksIfNull()
        {
            if (this.Tracks == null)
            {
                //fetch
                var cacheKey = $"album-tracks-{Id}";
                if (!SpotifySession.Cache.TryGetValue(cacheKey, out List<DiscTrack> fethed))
                {
                    var albumTracks =
                        await SpotifySession.Current.Api().PathFinder.QueryAlbumTracks(Id);
                    fethed =
                        albumTracks.Data.Album.Tracks.Items.Select((z, i) => new DiscTrack
                        {
                            Artists = z.Track.Artists.Items.Select(k => new TrackArtist
                            {
                                Name = k.Profile.Name,
                                Uri = k.Uri
                            }).ToList(),
                            ContextIndex = i + 1,
                            DiscNumber = z.Track.DiscNumber,
                            Playcount = int.Parse(z.Track.Playcount),
                            Duration = z.Track.Duration.TotalMilliseconds,
                            Uri = z.Track.Uri,
                            Name = z.Track.Name,
                        }).ToList();
                    SpotifySession.Cache.Set(cacheKey, fethed);
                }

                Tracks = fethed;
                //InvokeMethod(this, _tracks);
            }
        }

        [JsonIgnore]
        public List<DiscTrack> Tracks
        {
            get
            {
                return _tracks;
            }
            set
            {
                _tracks = value;
                OnPropertyChanged(nameof(Tracks));
            }
        }

        [JsonProperty("month")]
        public long? Month { get; set; }

        [JsonProperty("day")]
        public long? Day { get; set; }
    }

    public partial class Disc
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tracks")]
        public List<DiscTrack> Tracks { get; set; }
    }

    public partial class DiscTrack : GenericSpotifyItem
    {
        private List<TrackArtist> _artists;
        private int _numb;
        [JsonIgnore]
        public int DiscNumber { get; set; }


        [JsonProperty("playcount")]
        public int Playcount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonIgnore]
        public string ArtistsString { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("number")]
        public int Number
        {
            get => _numb;
            set
            {
                _numb = value;
              //  base.ContextIndex = value;
            }
        }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("playable")]
        public bool Playable { get; set; }

        [JsonProperty("artists")]
        public List<TrackArtist> Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                ArtistsString = string.Join(", ", value.Select(z => z.Name));
            }
        }
    }

    public partial class TrackArtist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public partial class AppearsOn
    {
        [JsonProperty("releases")]
        public List<LatestReleaseElement> Releases { get; set; }

        [JsonProperty("total_count")]
        public long? TotalCount { get; set; }
    }

    public partial class Compilations
    {
        [JsonProperty("total_count")]
        public long? TotalCount { get; set; }
    }

    public partial class TopTracks
    {
        [JsonProperty("tracks")]
        public List<TopTracksTrack> Tracks { get; set; }
    }

    public partial class TopTracksTrack : GenericSpotifyItem
    {
        [JsonProperty("playcount")]
        public long? Playcount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("release")]
        public TrackRelease Release { get; set; }

        [JsonProperty("explicit")]
        public bool? Explicit { get; set; }
    }

    public partial class TrackRelease
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }
    }

    public partial class UpcomingConcerts
    {
        [JsonProperty("inactive_artist")]
        public bool? InactiveArtist { get; set; }
    }
}