using System;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Events;
using SpotifyLib.Interfaces;
using SpotifyLib.Services;

namespace SpotifyLib.SpotifyConnect.Events
{
    internal sealed class NewSessionIdEvent : IGenericEvent
    {
        private readonly string sessionId;
        private readonly SpotifyState state;

        public NewSessionIdEvent(
            [NotNull] String sessionId,
            [NotNull] SpotifyState state)
        {
            this.sessionId = sessionId;
            this.state = state;
        }


        public EventBuilder Build()
        {
            var contextUri = state.ConnectState.ContextUri;

            var @event = new EventBuilder(EventType.NEW_SESSION_ID);
            @event.Append(sessionId);
            @event.Append(contextUri);
            @event.Append(contextUri);
            @event.Append(TimeProvider.CurrentTimeMillis().ToString());
            @event.Append("").Append(state.GetContextSize().ToString());
            @event.Append(state.ConnectState.ContextUrl);
            return @event;
        }
    }
}