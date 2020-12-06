using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify;
using SpotifyLib.Exceptions;
using SpotifyLib.Helpers;
using SpotifyLib.Interfaces;
using SpotifyLib.Listeners;
using SpotifyLib.Mercury;

namespace SpotifyLib.Services
{
    internal class SyncCallback : ICallback
    {
        private readonly System.Threading.EventWaitHandle _waitHandle = new System.Threading.AutoResetEvent(false);
        private MercuryResponse _reference;

        internal MercuryResponse WaitResponse()
        {
            _waitHandle.WaitOne();
            return _reference;
        }

        void ICallback.Response(MercuryResponse response)
        {
            _reference = response;
            _waitHandle.Set();
        }
    }

    public class MercuryClient : PacketsManager
    {
        private readonly ConcurrentDictionary<long, ICallback> _callbacks = new ConcurrentDictionary<long, ICallback>();
        private static readonly int MercuryRequestTimeout = 3000;
        private volatile int _seqHolder;
        private readonly SpotifySession _session;
        private readonly ConcurrentDictionary<long, BytesArrayList> _partials 
            = new ConcurrentDictionary<long, BytesArrayList>();
        private readonly ConcurrentBag<InternalSubListener> _subscriptions = 
            new ConcurrentBag<InternalSubListener>();
        private readonly System.Threading.EventWaitHandle _removeCallbackLock = new System.Threading.AutoResetEvent(false);
        internal MercuryClient(SpotifySession session) 
            : base(session, "pm-session")
        {
            _session = session;
            _seqHolder = 1;
        }
        internal void InterestedIn(
            [NotNull] string uri,
            [NotNull] ISubListener listener)
        {
            lock (_subscriptions)
            {
                _subscriptions.Add(new InternalSubListener(uri, listener, false));
            }
        }

        public MercuryResponse SendSync([NotNull] RawMercuryRequest request)
        {
            var callback = new SyncCallback();

            int seq = Send(request, callback);
            var resp = callback.WaitResponse();
            if (resp == null)
                throw new Exception(
                    $"Request timeout out, {MercuryRequestTimeout} passed, yet no response. seq: {seq}");
            return resp;
        }

        public T SendSync<T>([NotNull] JsonMercuryRequest<T> request) where T : class
        {
            var resp = SendSync(request.Request);
            if (resp.StatusCode >= 200 && resp.StatusCode < 300) return request.Instantiate(resp);
            else throw new MercuryException(resp);
        }
        public T SendSync<T>([NotNull] ProtobuffedMercuryRequest<T> request) where T : IMessage<T>
        {
            var resp = SendSync(request.Request);
            if (resp.StatusCode >= 200 && resp.StatusCode < 300) return request.Instantiate(resp);
            else throw new MercuryException(resp);
        }


        public int Send([NotNull] RawMercuryRequest request,
            [NotNull] ICallback callback)
        {
            var partial = new List<byte[]>();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var payloadNew = new List<byte[]>();
            var requestPayload = request._payload.ToArray();
            var requestHeader = request._header;
            if (requestPayload == null || requestHeader == null)
                throw new Exception("An unknown error occured. the librar could be outdated");

            var bytesOut = new MemoryStream();
            var s4B = (BitConverter.GetBytes((short) 4)).Reverse().ToArray();
            bytesOut.Write(s4B, 0, s4B.Length); // Seq length


            var seqB = BitConverter.GetBytes(Interlocked.Increment(ref _seqHolder)).Reverse().ToArray();
            bytesOut.Write(seqB, 0, seqB.Length); // Seq

            bytesOut.WriteByte(1); // Flags
            var reqpB = BitConverter.GetBytes((short) (1 + requestPayload.Length)).Reverse().ToArray();
            bytesOut.Write(reqpB, 0, reqpB.Length); // Parts count

            var headerBytes2 = requestHeader.ToByteArray();
            var hedBls = BitConverter.GetBytes((short) headerBytes2.Length).Reverse().ToArray();

            bytesOut.Write(hedBls, 0, hedBls.Length); // Header length
            bytesOut.Write(headerBytes2, 0, headerBytes2.Length); // Header

            foreach (byte[] part in requestPayload)
            {
                // Parts
                var l = BitConverter.GetBytes((short) part.Length).Reverse().ToArray();
                bytesOut.Write(l, 0, l.Length);
                bytesOut.Write(part, 0, part.Length);
            }

            var cmd = MercuryPacket.Type.MercuryReq;
            _session.Send(cmd, bytesOut.ToArray());
            _callbacks.TryAdd((long) _seqHolder, callback);
            return _seqHolder;
        }

        protected override void Handle(MercuryPacket packet)
        {
            var stream = new MemoryStream(packet.Payload);
            int seqLength = getShort(packet.Payload, (int)stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);
            long seq = 0;
            var buffer = packet.Payload;
            switch (seqLength)
            {
                case 2:
                    seq = getShort(packet.Payload, (int)stream.Position, true);
                    stream.Seek(2, SeekOrigin.Current);
                    break;
                case 4:
                    seq = getInt(packet.Payload, (int)stream.Position, true);
                    stream.Seek(4, SeekOrigin.Current);
                    break;
                case 8:
                    seq = getLong(packet.Payload, (int)stream.Position, true);
                    stream.Seek(8, SeekOrigin.Current);
                    break;
            }


            byte flags = packet.Payload[(int)stream.Position];
            stream.Seek(1, SeekOrigin.Current);
            short parts = getShort(packet.Payload, (int)stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);



            _partials.TryGetValue(seq, out var partial);
            if (partial == null || flags == 0)
            {
                partial = new BytesArrayList();
                _partials.TryAdd(seq, partial);
            }

            Debug.WriteLine($"Handling packet, cmd: {packet.Cmd}, seq: {seq}, flags: {flags}, parts: {parts}");

            for (int j = 0; j < parts; j++)
            {
                short size = getShort(packet.Payload, (int)stream.Position, true);
                stream.Seek(2, SeekOrigin.Current);

                byte[] buffer2 = new byte[size];

                int end = buffer2.Length;
                for (int z = 0; z < end; z++)
                {
                    byte a = packet.Payload[(int)stream.Position];
                    stream.Seek(1, SeekOrigin.Current);
                    buffer2[z] = a;
                }

                partial.Add(buffer2);
            }

            if (flags != 1) return;

            _partials.TryRemove(seq, out partial);

            Header header;
            try
            {
                header = Header.Parser.ParseFrom(partial.First());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't parse header! bytes: {Utils.bytesToHex(partial.First())}");
                throw ex;
            }

            var resp = new MercuryResponse(header, partial);

            switch (packet.Cmd)
            {
                case MercuryPacket.Type.MercuryEvent:
                    bool dispatched = false;
                    lock (_subscriptions)
                    {
                        foreach (var sub in _subscriptions)
                        {
                            if (sub.Matches(header.Uri))
                            {
                                sub.Dispatch(resp);
                                dispatched = true;
                            }
                        }
                    }

                    if (!dispatched)
                        Debug.WriteLine(
                            $"Couldn't dispatch Mercury event seq: {seq}, uri: {header.Uri}, code: {header.StatusCode}, payload: {resp.Payload.ToHex()}");

                    break;
                case MercuryPacket.Type.MercuryReq:
                case MercuryPacket.Type.MercurySub:
                case MercuryPacket.Type.MercuryUnsub:
                    _callbacks.TryRemove(seq, out var val);
                    if (val != null)
                    {
                        val.Response(resp);
                    }
                    else
                    {
                        Debug.WriteLine(
                            $"Skipped Mercury response, seq: {seq}, uri: {header.Uri}, code: {header.StatusCode}");
                    }

                    lock (_removeCallbackLock)
                    {
                        _removeCallbackLock.Reset();
                    }
                    break;
                default:
                    Debugger.Break();
                    break;
            }
        }

        protected override void Exception(Exception ex)
        {
            throw new NotImplementedException();
        }

        #region PRivates

        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)getShortL(obj0, obj1) : (int)getShortB(obj0, obj1));
        }
        private static short getShortB(byte[] obj0, int obj1) => makeShort(obj0[obj1], obj0[obj1 + 1]);

        private static short getShortL(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1 + 1], obj0[obj1]);
        }
        private static short makeShort(byte obj0, byte obj1) => (short)((int)(sbyte)obj0 << 8 | (int)(sbyte)obj1 & (int)byte.MaxValue);




        private static int getInt(byte[] obj0, int obj1, bool obj2) => !obj2 ? getIntL(obj0, obj1) : getIntB(obj0, obj1);

        private static int getIntB(byte[] obj0, int obj1) => makeInt(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3]);

        private static int getIntL(byte[] obj0, int obj1) => makeInt(obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

        private static int makeInt(byte obj0, byte obj1, byte obj2, byte obj3) => (int)(sbyte)obj0 << 24 | ((int)(sbyte)obj1 & (int)byte.MaxValue) << 16 | ((int)(sbyte)obj2 & (int)byte.MaxValue) << 8 | (int)(sbyte)obj3 & (int)byte.MaxValue;



        private static long getLong(byte[] obj0, int obj1, bool obj2) => !obj2 ? getLongL(obj0, obj1) : getLongB(obj0, obj1);

        private static long getLongB(byte[] obj0, int obj1) => makeLong(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3], obj0[obj1 + 4], obj0[obj1 + 5], obj0[obj1 + 6], obj0[obj1 + 7]);

        private static long getLongL(byte[] obj0, int obj1) => makeLong(obj0[obj1 + 7], obj0[obj1 + 6], obj0[obj1 + 5], obj0[obj1 + 4],obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

        private static long makeLong(
             byte obj0,
             byte obj1,
             byte obj2,
             byte obj3,
             byte obj4,
             byte obj5,
             byte obj6,
             byte obj7)
        {
            return (long)(sbyte)obj0 << 56 
                   | ((long)(sbyte)obj1 & (long)byte.MaxValue) << 48 
                   | ((long)(sbyte)obj2 & (long)byte.MaxValue) << 40 
                   | ((long)(sbyte)obj3 & (long)byte.MaxValue) << 32 
                   | ((long)(sbyte)obj4 & (long)byte.MaxValue) << 24 
                   | ((long)(sbyte)obj5 & (long)byte.MaxValue) << 16 
                   | ((long)(sbyte)obj6 & (long)byte.MaxValue) << 8 
                   | (long)(sbyte)obj7 & (long)byte.MaxValue;
        }
        #endregion
    }
}
