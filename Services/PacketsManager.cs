using System;
using JetBrains.Annotations;
using SpotifyLib.Mercury;

namespace SpotifyLib.Services
{
    public abstract class PacketsManager : IDisposable
    {
        protected readonly SpotifySession Session;
        private readonly Func<MercuryPacket, bool> _worker;

        internal PacketsManager([NotNull] SpotifySession session,
            string name)
        {
            Session = session;
            _worker = (packet) =>
            {
                try
                {
                    Handle(packet);
                }
                catch (Exception ex)
                {
                    Exception(ex);
                }
                return true;
            };
        }


        public void Dispatch(MercuryPacket packet) => AppendToQueue(packet);

        protected virtual void AppendToQueue([NotNull] MercuryPacket packet)
        {
            _worker.Invoke(packet);
        }

        protected abstract void Handle([NotNull] MercuryPacket packet);

        protected abstract void Exception([NotNull] Exception ex);

        public virtual void Dispose(bool dispose)
        {

        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
