using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpotifyLib.Services
{
    public static class TimeProvider
    {

        private static readonly object OffsetLock = new object();
        private static long _offset;

        public static void Init(SpotifySession sess)
        {
            _ = UpdateMelody(sess);
        }

        private static async Task UpdateMelody([NotNull] SpotifySession _sess)
        {
            var time = await _sess.Api().Melody.GetTime();
            var diff = time.timestamp - CurrentTimeMillisSystem();
            Interlocked.Exchange(ref _offset, diff);
        }

        public static long CurrentTimeMillis()
        {
            lock (OffsetLock)
            {
                return CurrentTimeMillisSystem() + _offset;
            }
        }
        private static readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillisSystem()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public enum Method
        {
            Ntp, Ping, Melody, Manual
        }
    }
}
