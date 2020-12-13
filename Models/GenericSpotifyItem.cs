using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpotifyLib.Enums;

namespace SpotifyLib.Models
{
    public class GenericSpotifyItem : INotifyPropertyChanged
    {
        private string _uri;
        private bool _isFocus;
        private bool _isCurrentlyPlaying;

        public GenericSpotifyItem(SpotifyType? typeOverride = null)
        {
            Type = typeOverride ?? SpotifyType.Track;
        }

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri
        {
            get => _uri;
            set
            {
                _uri = value;
                if (Type == SpotifyType.Track)
                {
                    switch (value.Split(':')[1])
                    {
                        case "track":
                            Type = SpotifyType.Track;
                            break;
                        case "artist":
                            Type = SpotifyType.Artist;
                            break;
                        case "album":
                            Type = SpotifyType.Album;
                            break;
                        case "show":
                            Type = SpotifyType.Show;
                            break;
                        case "episode":
                            Type = SpotifyType.Episode;
                            break;
                    }
                }

                Id ??= value.Split(':').LastOrDefault();
            }
        }

        [JsonIgnore]
        public SpotifyType Type { get; set; }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id
        {
            get;
            set;
        }


        [JsonIgnore]
        public bool IsCurrentlyPlaying
        {
            get => _isCurrentlyPlaying;
            set
            {
                _isCurrentlyPlaying = value;
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
            }
        }
        [JsonIgnore]
        public bool IsInFocus

        {
            get => _isFocus;
            set
            {
                _isFocus = value;
                OnPropertyChanged(nameof(IsInFocus));
            } 
        }

        public int ContextIndex
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
