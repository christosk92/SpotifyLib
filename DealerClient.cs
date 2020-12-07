using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Interfaces;
using SpotifyLib.Models;

namespace SpotifyLib
{
    public enum MessageType
    {
        ping,
        pong,
        message,
        request
    }

    //TODO: Port WebSocket
    public class DealerClient : IDisposable
    {
        private readonly SpotifySession _session;
        private static ApResolver _apResolver;

        private readonly ConcurrentDictionary<IMessageListener, List<string>> _msgListeners = new ConcurrentDictionary<IMessageListener, List<string>>();
        private readonly ManualResetEvent _msgListenersLock = new ManualResetEvent(false);

        private readonly ConcurrentDictionary<string, IRequestListener> _reqListeners = new ConcurrentDictionary<string, IRequestListener>();
        private readonly ManualResetEvent _reqListenersLock = new ManualResetEvent(false);


        private readonly WebsocketHandler _webSocket;
        private bool _closed = false;
        private bool _receivedPong = false;

        public DealerClient(SpotifySession session, WebsocketHandler websocket)
        {
            _session = session;
            _webSocket = websocket;
            _webSocket.MessageReceived += WebSocket_MessageReceived;
            _webSocket.SocketDisconnected += WebSocket_SocketDisconnected;
            _webSocket.SocketConnected += WebSocket_SocketConnected;
        }

        private static Dictionary<string, string> GetHeaders([NotNull] JObject obj)
        {
            var headers = obj["headers"] as JObject;
            return headers.ToObject<Dictionary<string, string>>();
        }

        public async Task Connect()
        {
            await _webSocket.ConnectSocketAsync(new Uri(
                $"wss://{(await _apResolver.GetClosestDealerAsync()).Replace("https://", string.Empty)}/?access_token={_session.Tokens().GetToken("playlist-read").AccessToken}"));
        }

        private async Task HandleRequest(JObject obj)
        {
            Debug.Assert(obj != null, nameof(obj) + " != null");
            var mid = obj["message_ident"]?.ToString();
            var key = obj["key"]?.ToString();
            var headers = GetHeaders(obj);
            var payload = obj["payload"];

            using var @in = new MemoryStream();
            using var outputStream = new MemoryStream(Convert.FromBase64String(payload["compressed"].ToString()));
            if (headers["Transfer-Encoding"]?.Equals("gzip") ?? false)
            {
                using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                decompressionStream.CopyTo(@in);
                Debug.WriteLine($"Decompressed");
                var jsonStr = Encoding.Default.GetString(@in.ToArray());
                payload = JObject.Parse(jsonStr);

            }

            var pid = payload["message_id"].ToObject<int>();
            var sender = payload["sent_by_device_id"]?.ToString();

            var command = payload["command"];
            Debug.WriteLine("Received request. mid: {0}, key: {1}, pid: {2}, sender: {3}", mid, key, pid, sender);
            var interesting = false;

            foreach (var midprefix in _reqListeners.Keys)
            {
                if (mid.StartsWith(midprefix))
                {
                    var listener = _reqListeners[midprefix];
                    interesting = true;
                    var result = listener.OnRequest(mid, pid, sender, (JObject)command);
                    await SendReply(key, result);
                    Debug.WriteLine("Handled request. key: {0}, result: {1}", key, result);
                }
            }
            if (!interesting) Debug.WriteLine("Couldn't dispatch request: " + mid);
        }

        async Task SendReply([NotNull] string key, [NotNull] RequestResult result)
        {
            var success = result == RequestResult.Success;
            var reply =
                $"{{\"type\":\"reply\", \"key\": \"{key.ToLower()}\", \"payload\": {{\"success\": {success.ToString().ToLowerInvariant()}}}}}";
            await _webSocket.SendMessageAsync(reply);
        }
        private void HandleMessage(JObject obj)
        {
            Debug.Assert(obj != null, nameof(obj) + " != null");
            var uri = obj["uri"]?.ToString();
            var headers = GetHeaders(obj);
            var payloads = (JArray)obj["payloads"];
            byte[] decodedPayload;
            if (payloads != null)
            {
                if (headers.ContainsKey("Content-Type")
                    && (headers["Content-Type"].Equals("application/json") ||
                        headers["Content-Type"].Equals("text/plain")))
                {
                    if (payloads.Count > 1) throw new InvalidOperationException();
                    decodedPayload = Encoding.Default.GetBytes(payloads[0].ToString());
                }
                else
                {
                    var payloadsStr = new string[payloads.Count];
                    for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                    var x = string.Join("", payloadsStr);
                    using var @in = new MemoryStream();
                    using var outputStream = new MemoryStream(Convert.FromBase64String(x));
                    if (headers.ContainsKey("Transfer-Encoding")
                        && (headers["Transfer-Encoding"]?.Equals("gzip") ?? false))
                    {
                        using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                        decompressionStream.CopyTo(@in);
                        Debug.WriteLine($"Decompressed");

                    }

                    decodedPayload = @in.ToArray();
                }
            }
            else
            {
                decodedPayload = new byte[0];
            }

            var interesting = false;

            lock (_msgListeners)
            {
                foreach (var listener in _msgListeners.Keys)
                {
                    var dispatched = false;
                    var keys = _msgListeners[listener];
                    foreach (var key
                        in
                        keys.Where(key => uri != null && uri.StartsWith(key) && !dispatched))
                    {
                        interesting = true;
                        listener.OnMessage(uri!, headers, decodedPayload);
                        dispatched = true;
                    }
                }
            }

            if (!interesting) Debug.WriteLine("Couldn't dispatch message: " + uri);
        }
        private void WaitForListeners()
        {
            lock (_msgListeners)
            {
                if (!_msgListeners.Any()) return;
            }

            try
            {
                _msgListenersLock.WaitOne();
            }
            catch (Exception)
            {
                //ignored
            }
        }
        public void AddMessageListener([NotNull] IMessageListener listener, [NotNull] params string[] uris)
        {
            lock (_msgListeners)
            {
                if (_msgListeners.ContainsKey(listener))
                    throw new ArgumentException($"A listener for {Arrays.ToString(uris)} has already been added.");

                _msgListeners.TryAdd(listener, uris.ToList());
                _msgListenersLock.Set();
            }
        }
        public void RemoveMessageListener([NotNull] IMessageListener listener)
        {
            lock (_msgListeners)
            {
                _msgListeners.TryRemove(listener, out var ignore);
            }
        }
        public void AddRequestListener([NotNull] IRequestListener listener, [NotNull] string uri)
        {
            lock (_reqListeners)
            {
                if (_reqListeners.ContainsKey(uri))
                    throw new ArgumentException($"A listener for {uri} has already been added.");

                _reqListeners.TryAdd(uri, listener);
                _reqListenersLock.Reset();
            }
        }
        public void RemoveRequestListener([NotNull] IRequestListener listener)
        {
            lock (_reqListeners)
            {
                _reqListeners.Values.Remove(listener);
            }
        }
        private void WebSocket_SocketConnected(object sender, string e)
        {
            if (_closed)
            {
                Debug.WriteLine("I wonder what happened here... Terminating. closed: {0}", _closed);
                return;
            }

            Debug.WriteLine("Dealer connected! host: {0}", e?.ToString());
        }
        private void WebSocket_SocketDisconnected(object sender,
            WebsocketclosedEventArgs e)
        {

        }
        private async void WebSocket_MessageReceived(object sender, string e)
        {
            var obj = JObject.Parse(e);
            WaitForListeners();

            if (!Enum.TryParse(obj["type"]?.ToString().ToLower(), out MessageType resolvedType))
                throw new ArgumentOutOfRangeException(nameof(MessageType), "Unknown message received");
            switch (resolvedType)
            {
                case MessageType.ping:
                    break;
                case MessageType.pong:
                    _receivedPong = true;
                    break;
                case MessageType.message:
                    try
                    {
                        HandleMessage(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed handling message: " + obj + " " + ex.ToString());
                    }
                    break;
                case MessageType.request:
                    try
                    {
                        await HandleRequest(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed handling Request: " + obj, ex);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void Dispose()
        {
            _msgListenersLock?.Dispose();
            _webSocket?.Dispose();
        }
    }
}