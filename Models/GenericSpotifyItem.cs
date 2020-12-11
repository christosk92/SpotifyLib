using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpotifyLib.Models
{
    public class GenericSpotifyItem : INotifyPropertyChanged
    {
        private string _uri;
        private bool _isFocus;
        private bool _isCurrentlyPlaying;

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri
        {
            get => _uri;
            set
            {
                _uri = value;
                Id ??= value.Split(':').LastOrDefault();
            }
        }

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
