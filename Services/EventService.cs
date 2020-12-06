using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.Events;
using SpotifyLib.Mercury;

namespace SpotifyLib.Services
{
    public class EventService : IDisposable
    {
        private readonly SpotifySession _session;

        internal EventService(SpotifySession session)
        {
            _session = session;
        }

        public void Language([NotNull] string lang)
        {
            EventBuilder @event = new EventBuilder(EventType.LANGUAGE);
            @event.Append(lang);
            SendEvent(@event);
        }

        public void SendEvent([NotNull] EventBuilder builder)
        {
            try
            {
                var body = builder.ToArray();
                var req = new RawMercuryRequest("hm://event-service/v1/events", "POST");
                req._payload.Add(body);
                req.AddUserField("Accept-Language", "en");
                req.AddUserField("X-ClientTimeStamp", TimeProvider.CurrentTimeMillis().ToString());

                MercuryResponse resp = _session.Mercury().SendSync(req);
                Debug.WriteLine(
                    $"Event sent. body: {EventBuilder.ToString(body)}, result: {resp.StatusCode.ToString()}");
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Failed sending event: " + builder + ex.ToString());
            }
        }

        public virtual void Dispose(bool dispose)
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
