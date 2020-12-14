﻿using Connectstate;
using Google.Protobuf;
using GuardAgainstLib;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Spotify;
using Spotify.ExplicitContent.Proto;
using SpotifyLib.Authenticators;
using SpotifyLib.Crypto;
using SpotifyLib.Exceptions;
using SpotifyLib.Helpers;
using SpotifyLib.Interfaces;
using SpotifyLib.Mercury;
using SpotifyLib.Models;
using SpotifyLib.Models.Api.Response;
using SpotifyLib.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyLib.Models.Mercury;
using SpotifyLib.SpotifyConnect;

namespace SpotifyLib
{
    internal class SocialHandler : ISubListener
    {
        private readonly SpotifySession _session;

        internal SocialHandler(SpotifySession inter)
        {
            _session = inter;
        }

        public void OnEvent(MercuryResponse resp)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(new MemoryStream(Combine(resp.Payload.ToArray())));
            using var jsonTextReader = new JsonTextReader(sr);
            var data = typeof(UserPresence) == typeof(string)
                ? (UserPresence) (object) sr.ReadToEnd()
                : serializer.Deserialize<UserPresence>(jsonTextReader);
            _session.IncomingPresence(data);
        }

        internal static byte[] Combine(byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
    }

    internal class Inner
    {
        internal readonly DeviceType DeviceType;
        internal readonly string DeviceName;
        internal readonly Random Random;
        internal readonly string DeviceId;
        internal readonly Configuration Conf;
        internal readonly string PreferredLocale;

        internal Inner(DeviceType deviceType,
            [NotNull] string deviceName,
            string deviceId,
            [NotNull] string preferredLocale,
            [NotNull] Configuration conf)
        {
            this.Random = new Random();
            this.PreferredLocale = preferredLocale;
            this.Conf = conf;
            this.DeviceType = deviceType;
            this.DeviceName = deviceName;
            this.DeviceId = (deviceId == null || deviceId.IsEmpty())
                ? Utils.RandomHexString(Random, 40).ToLower()
                : deviceId;
        }
    }

    public class SpotifySession : ISubListener, IMessageListener
    {
        /// <summary>
        /// Only use if neccesary!
        /// </summary>
        public static SpotifySession Current;

        private readonly Nito.AsyncEx.AsyncAutoResetEvent authLockEventWaitHandle = new AsyncAutoResetEvent(false);
        private readonly Nito.AsyncEx.AsyncLock authLock = new AsyncLock();

        private readonly byte[] serverKey = {
            (byte) 0xac, (byte) 0xe0, (byte) 0x46, (byte) 0x0b, (byte) 0xff, (byte) 0xc2, (byte) 0x30, (byte) 0xaf,
            (byte) 0xf4, (byte) 0x6b, (byte) 0xfe, (byte) 0xc3,
            (byte) 0xbf, (byte) 0xbf, (byte) 0x86, (byte) 0x3d, (byte) 0xa1, (byte) 0x91, (byte) 0xc6, (byte) 0xcc,
            (byte) 0x33, (byte) 0x6c, (byte) 0x93, (byte) 0xa1,
            (byte) 0x4f, (byte) 0xb3, (byte) 0xb0, (byte) 0x16, (byte) 0x12, (byte) 0xac, (byte) 0xac, (byte) 0x6a,
            (byte) 0xf1, (byte) 0x80, (byte) 0xe7, (byte) 0xf6,
            (byte) 0x14, (byte) 0xd9, (byte) 0x42, (byte) 0x9d, (byte) 0xbe, (byte) 0x2e, (byte) 0x34, (byte) 0x66,
            (byte) 0x43, (byte) 0xe3, (byte) 0x62, (byte) 0xd2,
            (byte) 0x32, (byte) 0x7a, (byte) 0x1a, (byte) 0x0d, (byte) 0x92, (byte) 0x3b, (byte) 0xae, (byte) 0xdd,
            (byte) 0x14, (byte) 0x02, (byte) 0xb1, (byte) 0x81,
            (byte) 0x55, (byte) 0x05, (byte) 0x61, (byte) 0x04, (byte) 0xd5, (byte) 0x2c, (byte) 0x96, (byte) 0xa4,
            (byte) 0x4c, (byte) 0x1e, (byte) 0xcc, (byte) 0x02,
            (byte) 0x4a, (byte) 0xd4, (byte) 0xb2, (byte) 0x0c, (byte) 0x00, (byte) 0x1f, (byte) 0x17, (byte) 0xed,
            (byte) 0xc2, (byte) 0x2f, (byte) 0xc4, (byte) 0x35,
            (byte) 0x21, (byte) 0xc8, (byte) 0xf0, (byte) 0xcb, (byte) 0xae, (byte) 0xd2, (byte) 0xad, (byte) 0xd7,
            (byte) 0x2b, (byte) 0x0f, (byte) 0x9d, (byte) 0xb3,
            (byte) 0xc5, (byte) 0x32, (byte) 0x1a, (byte) 0x2a, (byte) 0xfe, (byte) 0x59, (byte) 0xf3, (byte) 0x5a,
            (byte) 0x0d, (byte) 0xac, (byte) 0x68, (byte) 0xf1,
            (byte) 0xfa, (byte) 0x62, (byte) 0x1e, (byte) 0xfb, (byte) 0x2c, (byte) 0x8d, (byte) 0x0c, (byte) 0xb7,
            (byte) 0x39, (byte) 0x2d, (byte) 0x92, (byte) 0x47,
            (byte) 0xe3, (byte) 0xd7, (byte) 0x35, (byte) 0x1a, (byte) 0x6d, (byte) 0xbd, (byte) 0x24, (byte) 0xc2,
            (byte) 0xae, (byte) 0x25, (byte) 0x5b, (byte) 0x88,
            (byte) 0xff, (byte) 0xab, (byte) 0x73, (byte) 0x29, (byte) 0x8a, (byte) 0x0b, (byte) 0xcc, (byte) 0xcd,
            (byte) 0x0c, (byte) 0x58, (byte) 0x67, (byte) 0x31,
            (byte) 0x89, (byte) 0xe8, (byte) 0xbd, (byte) 0x34, (byte) 0x80, (byte) 0x78, (byte) 0x4a, (byte) 0x5f,
            (byte) 0xc9, (byte) 0x6b, (byte) 0x89, (byte) 0x9d,
            (byte) 0x95, (byte) 0x6b, (byte) 0xfc, (byte) 0x86, (byte) 0xd7, (byte) 0x4f, (byte) 0x33, (byte) 0xa6,
            (byte) 0x78, (byte) 0x17, (byte) 0x96, (byte) 0xc9,
            (byte) 0xc3, (byte) 0x2d, (byte) 0x0d, (byte) 0x32, (byte) 0xa5, (byte) 0xab, (byte) 0xcd, (byte) 0x05,
            (byte) 0x27, (byte) 0xe2, (byte) 0xf7, (byte) 0x10,
            (byte) 0xa3, (byte) 0x96, (byte) 0x13, (byte) 0xc4, (byte) 0x2f, (byte) 0x99, (byte) 0xc0, (byte) 0x27,
            (byte) 0xbf, (byte) 0xed, (byte) 0x04, (byte) 0x9c,
            (byte) 0x3c, (byte) 0x27, (byte) 0x58, (byte) 0x04, (byte) 0xb6, (byte) 0xb2, (byte) 0x19, (byte) 0xf9,
            (byte) 0xc1, (byte) 0x2f, (byte) 0x02, (byte) 0xe9,
            (byte) 0x48, (byte) 0x63, (byte) 0xec, (byte) 0xa1, (byte) 0xb6, (byte) 0x42, (byte) 0xa0, (byte) 0x9d,
            (byte) 0x48, (byte) 0x25, (byte) 0xf8, (byte) 0xb3,
            (byte) 0x9d, (byte) 0xd0, (byte) 0xe8, (byte) 0x6a, (byte) 0xf9, (byte) 0x48, (byte) 0x4d, (byte) 0xa1,
            (byte) 0xc2, (byte) 0xba, (byte) 0x86, (byte) 0x30,
            (byte) 0x42, (byte) 0xea, (byte) 0x9d, (byte) 0xb3, (byte) 0x08, (byte) 0x6c, (byte) 0x19, (byte) 0x0e,
            (byte) 0x48, (byte) 0xb3, (byte) 0x9d, (byte) 0x66,
            (byte) 0xeb, (byte) 0x00, (byte) 0x06, (byte) 0xa2, (byte) 0x5a, (byte) 0xee, (byte) 0xa1, (byte) 0x1b,
            (byte) 0x13, (byte) 0x87, (byte) 0x3c, (byte) 0xd7,
            (byte) 0x19, (byte) 0xe6, (byte) 0x55, (byte) 0xbd
        };

        private readonly DiffieHellman keys;
        private readonly Inner inner;
        private Task<SpotifyConnection> conn;
        // private volatile Shannon _sendCipher;
        //private volatile Shannon _recvCipher;

        private volatile Shannon sendCipher = new Shannon();
        private volatile Shannon recvCipher = new Shannon();
        private readonly Nito.AsyncEx.AsyncLock recvLock = new AsyncLock();
        private readonly Nito.AsyncEx.AsyncLock sendLock = new AsyncLock();
        private DealerClient dealer;
        private PrivateUser currentUser;

        //private AudioKeyManager _audioKeyManager;
        private MercuryClient mercuryClient;

        private TokenProvider tokenProvider;
        private APWelcome apWelcome;
        private ApiClient apiClient;

        //   private PlayableContentFeeder _contentFeeder;
        private CancellationToken closedToken;

        //  private ChannelManager _channelManager;
        //   private CdnManager _cdn;
        // private CacheManager _cache;
        public readonly ConcurrentDictionary<string, string> UserAttributes =
            new ConcurrentDictionary<string, string>();

        private EventService eventService;
        private volatile int recvNonce;
        private volatile int sendNonce;
        private Receiver receiver;

        private LoginCredentials loginCredentials;

        private readonly ConcurrentBag<IReconnectionListener> reconnectionListeners =
            new ConcurrentBag<IReconnectionListener>();

        public static IMemoryCache Cache;

        private SpotifySession([NotNull] Inner inner,
            CancellationToken closedToken,
            [CanBeNull] string address)
        {
            recvNonce = 0;
            sendNonce = 0;
            this.inner = inner;
            this.closedToken = closedToken;
            this.conn = SpotifyConnection.Create(
                inner.Conf,
                address);
            this.keys = new DiffieHellman(new System.Random());
            Debug.WriteLine($"Created new session! deviceId: {inner.DeviceId}, ap: {address}");
            Current = this;
        }

        public string DeviceId => inner?.DeviceId;
        public string DeviceName => inner?.DeviceName;
        public DeviceType? DeviceType => inner?.DeviceType;
        public string CountryCode { get; private set; }

        public event EventHandler<UserPresence> UserPresenceUpdated;
        internal void IncomingPresence(UserPresence e) => UserPresenceUpdated?.Invoke(this, e);
        public SpotifySession()
        {
        }

        public string Username => apWelcome.CanonicalUsername;

        public string Locale => inner.PreferredLocale;


        public async Task<PrivateUser> CurrentUser()
        {
            if (currentUser != null) return currentUser;
            var user = await Api().User.Current();
            currentUser = user;
            return currentUser;
        }

        /// <summary>
        /// Creates a new Authenticated Session
        /// </summary>
        /// <param name="authenticator"><see cref="IAuthenticator"/> and its implementations:
        /// <seealso cref="StoredAuthenticator"/>
        /// <seealso cref="UserPassAuthenticator"/>
        /// </param>
        /// <param name="cts">Cancellation token for the asynchronous operation.</param>
        /// <param name="deviceType"><see cref="DeviceType"/></param>
        /// <param name="deviceName">string to display on spotify connect</param>
        /// <param name="conf">Configuration file. See <see cref="Configuration"/></param>
        /// <param name="ws">An implementation of the abstract class: <see cref="WebsocketHandler"/></param>
        /// <param name="preferredLocale">2 letter locale code. Default = English</param>
        /// <param name="overrideAdress"></param>
        /// <returns><see cref="SpotifySession"/></returns>
        /// <exception cref="UnauthorizedAccessException">When trying to authenticated with <seealso cref="StoredAuthenticator"/> but no file is saved</exception>
        /// <exception cref="SpotifyAuthenticatedException">When credentials are invalid.</exception>
        public static async Task<SpotifySession> CreateAsync(
            IAuthenticator authenticator,
            CancellationToken cts,
            DeviceType deviceType,
            string deviceName,
            Configuration conf,
            WebsocketHandler ws,
            string preferredLocale = "en",
            string overrideAdress = null)
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
            GuardAgainst.ArgumentBeingNull(authenticator);
            //  TimeProvider.Init(conf);
            var deviceId = Utils.RandomHexString(new Random(), 40).ToLower();
            var session =
                new SpotifySession(
                    new Inner(deviceType,
                        deviceName,
                        deviceId,
                        preferredLocale,
                        conf),
                    cts,
                    overrideAdress)
                {
                    loginCredentials = await authenticator.Get()
                };
            await session.conn;
            session.Connect();
            await session.Authenticate(session.loginCredentials, ws);
            return session;
        }

        public ConnectHandler AttachSpotifyConnect(
            ISpotifyDevice device,
            uint initialVolume,
            int volumeSteps)
        {
            WaitAuthLock();
            var player = 
                new SpotifyPlayer(
                    this, 
                    device, 
                    initialVolume,
                    volumeSteps);

            var connectHandler 
                = new ConnectHandler(this, player);
            return connectHandler;
        }

        /// <summary>
        /// <exception cref="IOException"/>
        /// <exception cref="SpotifyAuthenticatedException"/>
        /// <exception cref="AccessViolationException"/>
        /// </summary>
        private void Connect()
        {
            #region ClientHello Setup

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var clientHello = new ClientHello
            {
                BuildInfo = new BuildInfo
                {
                    Platform = Platform.Win32X86,
                    Product = Product.Client,
                    ProductFlags = { ProductFlags.ProductFlagNone },
                    Version = 112800721
                }
            };
            clientHello.CryptosuitesSupported.Add(Cryptosuite.Shannon);
            clientHello.LoginCryptoHello = new LoginCryptoHelloUnion
            {
                DiffieHellman = new LoginCryptoDiffieHellmanHello
                {
                    Gc = ByteString.CopyFrom(keys.PublicKeyArray()),
                    ServerKeysKnown = 1
                }
            };

            var nonce = new byte[16];
            (new Random()).NextBytes(nonce);
            clientHello.ClientNonce = ByteString.CopyFrom(nonce);
            clientHello.Padding = ByteString.CopyFrom(new byte[1]
            {
                (byte) 30
            });

            var clientHelloBytes = clientHello.ToByteArray();

            var a = conn.Result.NetworkStream;
            a.WriteByte(0x00);
            a.WriteByte(0x04);
            a.WriteByte(0x00);
            a.WriteByte(0x00);
            a.WriteByte(0x00);
            a.Flush();

            var length = 2 + 4 + clientHelloBytes.Length;
            var bytes = BitConverter.GetBytes(length);
            a.WriteByte(bytes[0]);
            a.Write(clientHelloBytes, 0, clientHelloBytes.Length);
            a.Flush();
            var buffer = new byte[1000];
            var len = int.Parse(a.Read(buffer, 0, buffer.Length).ToString());
            var tmp = new byte[len];
            Array.Copy(buffer, tmp, len);

            tmp = tmp.Skip(4).ToArray();
            var accumulator = new MemoryStream();
            accumulator.WriteByte(0x00);
            accumulator.WriteByte(0x04);

            var lnarr = Utils.toByteArray(length);
            accumulator.Write(lnarr, 0, lnarr.Length);
            accumulator.Write((byte[])clientHelloBytes, 0, clientHelloBytes.Length);

            var lenArr = Utils.toByteArray(len);
            accumulator.Write(lenArr, 0, lenArr.Length);
            accumulator.Write((byte[])tmp, 0, tmp.Length);

            #endregion ClientHello Setup

            //Read APResponseMessage

            #region APResponse

            var binaryData = accumulator.ToArray();
            var apResponseMessage = APResponseMessage.Parser.ParseFrom(tmp);
            var sharedKey = Utils.toByteArray(keys.ComputeSharedKey(apResponseMessage
                .Challenge.LoginCryptoChallenge.DiffieHellman.Gs.ToByteArray()));

            // Check gs_signature
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = new RSAParameters
            {
                Modulus = new BigInteger(1, serverKey).ToByteArrayUnsigned(),
                Exponent = BigInteger.ValueOf(65537).ToByteArrayUnsigned()
            };

            //Set  to the public key values.

            //Import key parameters into RSA.
            rsa.ImportParameters(rsaKeyInfo);
            var gs = apResponseMessage.Challenge.LoginCryptoChallenge.DiffieHellman.Gs.ToByteArray();
            var sign = apResponseMessage.Challenge.LoginCryptoChallenge.DiffieHellman.GsSignature.ToByteArray();

            if (!rsa.VerifyData(gs,
                sign,
                HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1))
                throw new AccessViolationException("Failed to verify APResponse");

            // Solve challenge
            binaryData = accumulator.ToArray();
            using var data = new MemoryStream();
            var mac = new HMACSHA1(sharedKey);
            mac.Initialize();
            for (var i = 1; i < 6; i++)
            {
                mac.TransformBlock(binaryData, 0, binaryData.Length, null, 0);
                var temp = new[] { (byte)i };
                mac.TransformBlock(temp, 0, temp.Length, null, 0);
                mac.TransformFinalBlock(new byte[0], 0, 0);
                var final = mac.Hash;
                data.Write(final, 0, final.Length);
                mac = new HMACSHA1(sharedKey);
            }

            var dataArray = data.ToArray();
            mac = new HMACSHA1(Arrays.CopyOfRange(dataArray, 0, 0x14));
            mac.TransformBlock(binaryData, 0, binaryData.Length, null, 0);
            mac.TransformFinalBlock(new byte[0], 0, 0);
            var challenge = mac.Hash;
            var clientResponsePlaintext = new ClientResponsePlaintext
            {
                LoginCryptoResponse = new LoginCryptoResponseUnion
                {
                    DiffieHellman = new LoginCryptoDiffieHellmanResponse
                    {
                        Hmac = ByteString.CopyFrom(challenge)
                    }
                },
                PowResponse = new PoWResponseUnion(),
                CryptoResponse = new CryptoResponseUnion()
            };
            var clientResponsePlaintextBytes = clientResponsePlaintext.ToByteArray();
            len = 4 + clientResponsePlaintextBytes.Length;
            a.WriteByte(0x00);
            a.WriteByte(0x00);
            a.WriteByte(0x00);
            var bytesb = BitConverter.GetBytes(len);
            a.WriteByte(bytesb[0]);
            a.Write(clientResponsePlaintextBytes, 0, clientResponsePlaintextBytes.Length);
            a.Flush();
            try
            {
                var scrap = new byte[4];
                conn.Result.NetworkStream.ReadTimeout = 300;
                var read = conn.Result.NetworkStream.Read(scrap, 0, scrap.Length);
                if (read == scrap.Length)
                {
                    length = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) | (scrap[3] & 0xFF);
                    var payload = new byte[length - 4];
                    conn.Result.NetworkStream.ReadComplete(payload, 0, payload.Length);
                    var failed = APResponseMessage.Parser.ParseFrom(payload)?.LoginFailed;
                    throw new SpotifyAuthenticatedException(failed);
                }
                else if (read > 0)
                {
                    throw new Exception("Read unknown data!");
                }
            }
            catch (Exception x)
            {
                // ignored
            }
            finally
            {
                conn.Result.NetworkStream.ReadTimeout = Timeout.Infinite;
            }

            using (authLock.Lock())
            {
                sendCipher = new Shannon();
                sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

                recvCipher = new Shannon();
                recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
                authLockEventWaitHandle.Set();
            }

            #endregion APResponse

            Debug.WriteLine("Connected successfully");
        }

        /// <summary>
        /// Authenticates with the server and creates all the necessary components.
        /// All of them should be initialized inside the synchronized block and MUST NOT call any method on <see cref="SpotifySession">this</see> object.
        /// </summary>
        /// <param name="credentials"><see cref="Spotify.LoginCredentials"/></param>
        /// <exception cref="MercuryException"></exception>
        private async Task Authenticate(
            [NotNull] Spotify.LoginCredentials credentials,
            [NotNull] WebsocketHandler handler)
        {
            AuthenticatePartial(credentials, false);

            using (await authLock.LockAsync())
            {
                //Initialize Services
                mercuryClient = new MercuryClient(this);
                eventService = new EventService(this);
                tokenProvider = new TokenProvider(this);
                apiClient = await ApiClient.BuildApiClient(this);
                dealer = new DealerClient(this, handler);
                //_audioKeyManager = new AudioKeyManager(this);
                //  _channelManager = new ChannelManager(this);
                //  _cdn = new CdnManager(this);
                // _cache = new CacheManager(_inner.Conf);
                //   _contentFeeder = new PlayableContentFeeder(this);
                authLockEventWaitHandle.Set();
            }

            receiver = new Receiver(this);

            eventService.Language(inner.PreferredLocale);
            TimeProvider.Init(this);

            Debug.WriteLine($"Authenticated as {apWelcome.CanonicalUsername}!");
            Mercury().InterestedIn("spotify:user:attributes:update", this);
            Dealer().AddMessageListener(this, "hm://connect-state/v1/connect/logout");
        }

        /// <summary>
        /// Authenticates with the server. Does not create all the components unlike <see cref="Authenticate"/>.
        /// </summary>
        /// <param name="credentials"><see cref="Spotify.LoginCredentials"/></param>
        /// <param name="removeLock">Whether <see cref="authLockEventWaitHandle"/> should be released or not.
        /// <code>false</code> for <see cref="Authenticate"/>
        /// <code>true</code> for <see cref="Reconnect"/></param>
        private void AuthenticatePartial(
            [NotNull] Spotify.LoginCredentials credentials,
            bool removeLock)
        {
            GuardAgainst.ArgumentBeingNull(recvCipher);
            GuardAgainst.ArgumentBeingNull(sendCipher);

            var clientResponseEncrypted = new ClientResponseEncrypted
            {
                LoginCredentials = credentials,
                SystemInfo = new SystemInfo
                {
                    Os = Os.Windows,
                    CpuFamily = CpuFamily.CpuX86,
                    SystemInformationString = "1",
                    DeviceId = inner.DeviceId
                },
                VersionString = "1.0"
            };
            SendUnchecked(MercuryPacket.Type.Login, clientResponseEncrypted.ToByteArray(), closedToken);

            var packet = Receive(conn.Result.NetworkStream, closedToken);
            switch (packet.Cmd)
            {
                case MercuryPacket.Type.APWelcome:
                    {
                        apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);
                        var bytes0X0F = new byte[20];
                        (new Random()).NextBytes(bytes0X0F);
                        SendUnchecked(MercuryPacket.Type.Unknown_0x0f, bytes0X0F, closedToken);

                        using var preferredLocale = new MemoryStream(18 + 5);
                        preferredLocale.WriteByte((byte)0x0);
                        preferredLocale.WriteByte((byte)0x0);
                        preferredLocale.WriteByte((byte)0x10);
                        preferredLocale.WriteByte((byte)0x0);
                        preferredLocale.WriteByte((byte)0x02);
                        preferredLocale.Write("preferred-locale");
                        preferredLocale.Write(inner.PreferredLocale);
                        SendUnchecked(MercuryPacket.Type.PreferredLocale, preferredLocale.ToArray(), closedToken);
                        if (removeLock)
                        {
                            using (authLock.Lock())
                            {
                                authLockEventWaitHandle.Set();
                            }
                        }

                        try
                        {
                            if (inner.Conf.StoreCredentials)
                            {
                                var jsonObj = new StoredCredentials
                                {
                                    AuthenticationType = apWelcome.ReusableAuthCredentialsType,
                                    Base64Credentials = apWelcome.ReusableAuthCredentials.ToBase64(),
                                    Username = apWelcome.CanonicalUsername
                                };
                                inner.Conf.StoreCredentialsFunction(jsonObj);
                                //  ApplicationData.Current.LocalSettings.Values["auth_data"] =
                                // JsonConvert.SerializeObject(jsonObj);
                            }
                        }
                        catch (Exception x)
                        {
                            Debug.WriteLine(x.ToString());
                            throw;
                        }

                        break;
                    }
                case MercuryPacket.Type.AuthFailure:
                    throw new SpotifyAuthenticatedException(APLoginFailed.Parser.ParseFrom(packet.Payload));
                default:
                    throw new Exception("Unknown CMD 0x" + packet.Cmd);
            }
        }

        public Configuration Configuration() => inner?.Conf;

        public Task AttachSocial()
            => Task.Run(() =>
            {
                var handler = new SocialHandler(this);
                var usersSubscribed =
                    Singleton<SpotifySession>.Instance.Mercury()
                        .SendSync(new ProtobuffedMercuryRequest<Spotify.Social.UserListReply>(
                            RawMercuryRequest.Get(
                                $"hm://socialgraph/subscriptions/user/{Username}?count=200&last_result="),
                            Spotify.Social.UserListReply.Parser));
                foreach (var user in usersSubscribed.Users)
                {
                    var response = Singleton<SpotifySession>.Instance.Mercury()
                        .SendSync(new JsonMercuryRequest<UserPresence>(
                            RawMercuryRequest.Get($"hm://presence2/user/{user.Username}")));
                    UserPresenceUpdated?.Invoke(this, response);
                    Singleton<SpotifySession>.Instance.Mercury()
                        .Subscribe($"hm://presence2/user/{user.Username}",
                            handler);
                }
            }, closedToken);

        public DealerClient Dealer()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(dealer, exceptionMessage: "Session isn't authenticated");
            return dealer;
        }

        /* public AudioKeyManager AudioKey()
         {
             WaitAuthLock();
             GuardAgainst.ArgumentBeingNull(_audioKeyManager, exceptionMessage: "Session isn't authenticated");
             return _audioKeyManager;
         }*/

        public MercuryClient Mercury()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(mercuryClient, exceptionMessage: "Session isn't authenticated");
            return mercuryClient;
        }

        // public ChannelManager Channel()
        // {
        //   WaitAuthLock();
        //   GuardAgainst.ArgumentBeingNull(_channelManager, exceptionMessage: "Session isn't authenticated");
        //   return _channelManager;
        // }

        public TokenProvider Tokens()
        {
            WaitAuthLock();
            if (tokenProvider == null) throw new Exception("Session isn't authenticated!");
            return tokenProvider;
        }

        public ApiClient Api()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(apiClient, exceptionMessage: "Session isn't authenticated");
            return apiClient;
        }

        //   public PlayableContentFeeder ContentFeeder()
        // {
        //     WaitAuthLock();
        //    GuardAgainst.ArgumentBeingNull(_contentFeeder, exceptionMessage: "Session isn't authenticated");
        //    return _contentFeeder;
        //}

        public EventService EventService()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(eventService, exceptionMessage: "Session isn't authenticated");
            return eventService;
        }

        //  public CdnManager Cdn()
        //{
        //    WaitAuthLock();
        //   GuardAgainst.ArgumentBeingNull(_cdn, exceptionMessage: "Session isn't authenticated");
        //   return _cdn;
        //  }

        // public CacheManager CacheManager()
        // {
        //    WaitAuthLock();
        //   GuardAgainst.ArgumentBeingNull(_cache, exceptionMessage: "Session isn't authenticated");
        //   return _cache;
        // }
        internal void Send(MercuryPacket.Type cmd, byte[] payload)
        {
            if (closedToken.IsCancellationRequested)
            {
                Debug.WriteLine("Connection was broken while Session.close() has been called");
                return;
            }

            using (authLock.Lock())
            {
                if (sendCipher == null)
                {
                    try
                    {
                        authLockEventWaitHandle.Wait(closedToken);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }

                SendUnchecked(cmd,
                    payload,
                    CancellationToken.None);
            }
        }

        private void SendUnchecked(MercuryPacket.Type cmd, byte[] payload, CancellationToken cts)
        {
            using (sendLock.Lock(cts))
            {
                var a = conn.Result.NetworkStream;
                var payloadLengthAsByte = BitConverter.GetBytes((short)payload.Length).Reverse().ToArray();
                using var yetAnotherBuffer = new MemoryStream(3 + payload.Length);
                yetAnotherBuffer.WriteByte((byte)cmd);
                yetAnotherBuffer.Write(payloadLengthAsByte, 0, payloadLengthAsByte.Length);
                yetAnotherBuffer.Write(payload, 0, payload.Length);

                sendCipher.nonce(Utils.toByteArray(sendNonce));
                Interlocked.Increment(ref sendNonce);

                var bufferBytes = yetAnotherBuffer.ToArray();
                sendCipher.encrypt(bufferBytes);

                var fourBytesBuffer = new byte[4];
                sendCipher.finish(fourBytesBuffer);
                a.Write(bufferBytes, 0, bufferBytes.Length);
                a.Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                a.Flush();
            }
        }

        internal MercuryPacket Receive(NetworkStream a, CancellationToken cts)
        {
            using (recvLock.Lock(cts))
            {
                recvCipher.nonce(Utils.toByteArray(recvNonce));
                Interlocked.Increment(ref recvNonce);

                var headerBytes = new byte[3];
                a.ReadComplete(headerBytes, 0, headerBytes.Length);
                recvCipher.decrypt(headerBytes);

                var cmd = headerBytes[0];
                var payloadLength = (short)((headerBytes[1] << 8) | (headerBytes[2] & 0xFF));

                var payloadBytes = new byte[payloadLength];
                a.ReadComplete(payloadBytes, 0, payloadBytes.Length);
                recvCipher.decrypt(payloadBytes);

                var mac = new byte[4];
                a.ReadComplete(mac, 0, mac.Length);

                var expectedMac = new byte[4];
                recvCipher.finish(expectedMac);
                return new MercuryPacket((MercuryPacket.Type)cmd, payloadBytes);
            }
        }

        private void Reconnect()
        {
            lock (reconnectionListeners)
            {
                foreach (var reconnectionListener in reconnectionListeners)
                {
                    reconnectionListener.OnConnectionDropped();
                }
            }

            if (conn != null)
            {
                conn.Result.Conn.Close();
                receiver.Stop();
            }

            try
            {
                conn = SpotifyConnection.Create(inner.Conf);
                Connect();
                AuthenticatePartial(new LoginCredentials
                {
                    Typ = apWelcome.ReusableAuthCredentialsType,
                    Username = apWelcome.CanonicalUsername,
                    AuthData = apWelcome.ReusableAuthCredentials
                }, true);
                Debug.WriteLine($"Re-authenticated as {apWelcome.CanonicalUsername}!");
                lock (reconnectionListeners)
                {
                    foreach (var reconnectionListener in reconnectionListeners)
                    {
                        reconnectionListener.OnConnectionEstablished();
                    }
                }
            }
            catch (Exception x)
            {
                Debugger.Break();
            }
        }

        private void WaitAuthLock()
        {
            if (closedToken.IsCancellationRequested)
            {
                Debug.WriteLine("Connection was broken while Session.close() has been called");
                return;
            }

            using (authLock.Lock())
            {
                if (sendCipher != null && recvCipher != null) return;
                authLockEventWaitHandle.Wait(closedToken);
            }
        }

        public void OnEvent(MercuryResponse resp)
        {
            
            if (!resp.Uri.Equals("spotify:user:attributes:update")) return;
            UserAttributesUpdate attributesUpdate;
            try
            {
                attributesUpdate = UserAttributesUpdate.Parser.ParseFrom(resp.Payload.Stream());
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Failed parsing user attributes update. " + ex.ToString());
                return;
            }

            foreach (var pair in attributesUpdate.Pairs)
            {
                UserAttributes.AddOrUpdate(pair.Key, pair.Value);
                Debug.WriteLine($"Updated user attribute: {pair.Key} -> {pair.Value}");
            }
        }

        internal class Receiver
        {
            private readonly CancellationTokenSource cts;
            private readonly SpotifySession session;

            internal Receiver(SpotifySession session)
            {
                this.session = session;
                cts = new CancellationTokenSource();
                var ts = new ThreadStart(BackgroundMethod);
                var backgroundThread = new Thread(ts);
                backgroundThread.Start();
            }

            public void Stop() => cts.Cancel();

            private static void ParseProductInfo(byte[] @in)
            {
                Debug.WriteLine(Encoding.Default.GetString(@in));
            }

            private async void BackgroundMethod()
            {
                Debug.WriteLine("Session.Receiver started");
                while (!cts.IsCancellationRequested)
                {
                    MercuryPacket packet;
                    MercuryPacket.Type cmd;
                    var tokenCanceled = false;
                    try
                    {
                        try
                        {
                            packet = session.Receive(session.conn.Result.NetworkStream, cts.Token);
                            if (!Enum.TryParse(packet.Cmd.ToString(), out cmd))
                            {
                                Debug.WriteLine(
                                    $"Skipping unknown command cmd: {packet.Cmd}, payload: {Utils.bytesToHex(packet.Payload)}");
                                continue;
                            }

                            switch (cmd)
                            {
                                case MercuryPacket.Type.Ping:
                                    try
                                    {
                                        session.Send(MercuryPacket.Type.Pong, packet.Payload);
                                    }
                                    catch (IOException ex)
                                    {
                                        Debug.WriteLine("Failed sending Pong!", ex);
                                    }
                                    break;

                                case MercuryPacket.Type.PongAck:
                                    break;

                                case MercuryPacket.Type.CountryCode:
                                    session.CountryCode = Encoding.Default.GetString(packet.Payload);
                                    Debug.WriteLine("Received CountryCode: " + session.CountryCode);
                                    break;

                                case MercuryPacket.Type.LicenseVersion:
                                    Debug.WriteLine($"Received LicenseVersion: {Encoding.Default.GetString(packet.Payload)}");
                                    //ByteBuffer licenseVersion = ByteBuffer.wrap(packet.Payload);
                                    // short id = licenseVersion.getShort();
                                    // if (id != 0)
                                    //{
                                    //    byte[] buffer = new byte[licenseVersion.get()];
                                    //   licenseVersion.get(buffer);
                                    //LogManager.Log($"Received LicenseVersion: {id}, {Encoding.Default.GetString(buffer)}");
                                    // }
                                    // else
                                    //{
                                    //    LogManager.Log($"Received LicenseVersion: {id}");
                                    // }
                                    break;

                                case MercuryPacket.Type.MercuryReq:
                                case MercuryPacket.Type.MercurySub:
                                case MercuryPacket.Type.MercuryUnsub:
                                case MercuryPacket.Type.MercuryEvent:
                                    session.Mercury().Dispatch(packet);
                                    break;

                                case MercuryPacket.Type.AesKey:
                                case MercuryPacket.Type.AesKeyError:
                                    // _session.AudioKey().Dispatch(packet);
                                    break;

                                case MercuryPacket.Type.ChannelError:
                                case MercuryPacket.Type.StreamChunkRes:
                                    //_session.Channel().Dispatch(packet);
                                    break;

                                case MercuryPacket.Type.ProductInfo:
                                    try
                                    {
                                        ParseProductInfo(packet.Payload);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Failed parsing prodcut info!" + ex.ToString());
                                    }

                                    break;

                                case MercuryPacket.Type.Unknown_0x10:
                                    Debug.WriteLine("Received 0x10 : " + Utils.bytesToHex(packet.Payload));
                                    break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // The token was canceled and the semaphore was NOT entered...
                            tokenCanceled = true;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            Debug.WriteLine("Failed reading packet!" + ex.ToString());
                            session.Reconnect();
                        }

                        break;
                    }

                    if (cts.IsCancellationRequested)
                    {
                        break;
                    }
                }
                Debug.WriteLine("Session.Receiver Stopped");
            }
        }

        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            if (uri.Equals("hm://connect-state/v1/connect/logout"))
            {
                try
                {
                    //close();
                    Debugger.Break();
                }
                catch (IOException ex)
                {
                    // LOGGER.error("Failed closing session due to logout.", ex);
                }
            }

            return Task.FromResult("");
        }
    }
}