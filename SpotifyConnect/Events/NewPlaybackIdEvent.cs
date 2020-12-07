using System;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Events;
using SpotifyLib.Interfaces;
using SpotifyLib.Services;

namespace SpotifyLib.SpotifyConnect.Events
{
    internal class NewPlaybackIdEvent : IGenericEvent
    {
        private readonly string sessionId;
        private readonly string playbackId;

        internal NewPlaybackIdEvent([NotNull] string sessionId, [NotNull] string playbackId)
        {
            this.sessionId = sessionId;
            this.playbackId = playbackId;
        }

        public EventBuilder Build()
        {
            var @event = new EventBuilder(EventType.NEW_PLAYBACK_ID);
            @event.Append(playbackId).Append(sessionId)
                .Append(TimeProvider.CurrentTimeMillis().ToString());
            return @event;
        }
    }
}