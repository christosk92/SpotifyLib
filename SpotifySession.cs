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
using Connectstate;
using Google.Protobuf;
using GuardAgainstLib;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
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

namespace SpotifyLib
{
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

        private readonly Nito.AsyncEx.AsyncAutoResetEvent _authLockEventWaitHandle = new AsyncAutoResetEvent(false);
        private readonly Nito.AsyncEx.AsyncLock _authLock = new AsyncLock();

        private readonly byte[] _serverKey = new byte[]
        {
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
        private readonly DiffieHellman _keys;
        private readonly Inner _inner;
        private Task<SpotifyConnection> _conn;
        // private volatile Shannon _sendCipher;
        //private volatile Shannon _recvCipher;

        private volatile Shannon _sendCipher = new Shannon();
        private volatile Shannon _recvCipher = new Shannon();
        private Nito.AsyncEx.AsyncLock _recvLock = new AsyncLock();
        private Nito.AsyncEx.AsyncLock _sendLock = new AsyncLock();
        private DealerClient _dealer;
        private PrivateUser _currentUser;
        //private AudioKeyManager _audioKeyManager;
        private MercuryClient _mercuryClient;
        private TokenProvider _tokenProvider;
        private APWelcome _apWelcome;
        private ApiClient _apiClient;
     //   private PlayableContentFeeder _contentFeeder;
        private CancellationToken _closedToken;
      //  private ChannelManager _channelManager;
       //   private CdnManager _cdn;
       // private CacheManager _cache;
        public readonly ConcurrentDictionary<string, string> UserAttributes =
            new ConcurrentDictionary<string, string>();

        private EventService _eventService;
        private volatile int _recvNonce;
        private volatile int _sendNonce;
        private Receiver _receiver;

        private LoginCredentials _loginCredentials;

        private readonly ConcurrentBag<IReconnectionListener> _reconnectionListeners =
            new ConcurrentBag<IReconnectionListener>();

        //These event handlers act as proxy events to communicate with the player from the viewmodel.
        /// <summary>
        /// If the parameter is set to true. The object has been paused
        /// </summary>
        public event EventHandler<ClusterUpdate> CurrentlyPlayingChanged;

        public static IMemoryCache Cache;
        public void OnCurrentlyPlayingChanged(ClusterUpdate val)
        {
            CurrentlyPlayingChanged?.Invoke(this, val);
        }
        private SpotifySession([NotNull] Inner inner,
            CancellationToken closedToken,
            [CanBeNull]string address)
        {
            _recvNonce = 0;
            _sendNonce = 0;
            this._inner = inner;
            _closedToken = closedToken;
            this._conn = SpotifyConnection.Create(
                inner.Conf,
                address);
            this._keys = new DiffieHellman(new System.Random());
            Debug.WriteLine($"Created new session! deviceId: {inner.DeviceId}, ap: {address}");
            Current = this;
        }
        public string DeviceId => _inner?.DeviceId;
        public string DeviceName => _inner?.DeviceName;
        public DeviceType? DeviceType => _inner?.DeviceType;
        public string CountryCode { get; private set; }

        public SpotifySession()
        {

        }

        public string Username => _apWelcome.CanonicalUsername;

        public string Locale => _inner.PreferredLocale;

        public async Task<PrivateUser> CurrentUser()
        {
            if (_currentUser == null)
            {
                var user = await Api().User.Current();
                _currentUser = user;
            }
            return _currentUser;
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
                    _loginCredentials = await authenticator.Get()
                };
            await session._conn;
            session.Connect();
            await session.Authenticate(session._loginCredentials);
            return session;
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
                    ProductFlags = {ProductFlags.ProductFlagNone},
                    Version = 112800721
                }
            };
            clientHello.CryptosuitesSupported.Add(Cryptosuite.Shannon);
            clientHello.LoginCryptoHello = new LoginCryptoHelloUnion
            {
                DiffieHellman = new LoginCryptoDiffieHellmanHello
                {
                    Gc = ByteString.CopyFrom(_keys.PublicKeyArray()),
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

            var a = _conn.Result.NetworkStream;
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
            accumulator.Write((byte[]) clientHelloBytes, 0, clientHelloBytes.Length);

            var lenArr = Utils.toByteArray(len);
            accumulator.Write(lenArr, 0, lenArr.Length);
            accumulator.Write((byte[]) tmp, 0, tmp.Length);

            #endregion

            //Read APResponseMessage

            #region APResponse

            var binaryData = accumulator.ToArray();
            var apResponseMessage = APResponseMessage.Parser.ParseFrom(tmp);
            var sharedKey = Utils.toByteArray(_keys.ComputeSharedKey(apResponseMessage
                .Challenge.LoginCryptoChallenge.DiffieHellman.Gs.ToByteArray()));


            // Check gs_signature
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = new RSAParameters
            {
                Modulus = new BigInteger(1, _serverKey).ToByteArrayUnsigned(),
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
                var temp = new byte[] {(byte) i};
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
                _conn.Result.NetworkStream.ReadTimeout = 300;
                var read = _conn.Result.NetworkStream.Read(scrap, 0, scrap.Length);
                if (read == scrap.Length)
                {
                    length = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) | (scrap[3] & 0xFF);
                    var payload = new byte[length - 4];
                    _conn.Result.NetworkStream.ReadComplete(payload, 0, payload.Length);
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
                _conn.Result.NetworkStream.ReadTimeout = Timeout.Infinite;
            }

            using(_authLock.Lock())
            {
                _sendCipher = new Shannon();
                _sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

                _recvCipher = new Shannon();
                _recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
                _authLockEventWaitHandle.Set();
            }

            #endregion

            Debug.WriteLine("Connected successfully");
        }

        /// <summary>
        /// Authenticates with the server and creates all the necessary components.
        /// All of them should be initialized inside the synchronized block and MUST NOT call any method on <see cref="SpotifySession">this</see> object.
        /// </summary>
        /// <param name="credentials"><see cref="Spotify.LoginCredentials"/></param>
        /// <exception cref="MercuryException"></exception>
        private async Task Authenticate([NotNull] Spotify.LoginCredentials credentials)
        {
            AuthenticatePartial(credentials, false);

            using (await _authLock.LockAsync())
            {
                //Initialize Services
                _mercuryClient = new MercuryClient(this);
                _eventService = new EventService(this);
                _tokenProvider = new TokenProvider(this);
                _apiClient = await ApiClient.BuildApiClient(this);
                _dealer = new DealerClient(this);
                //_audioKeyManager = new AudioKeyManager(this);
              //  _channelManager = new ChannelManager(this);
              //  _cdn = new CdnManager(this);
               // _cache = new CacheManager(_inner.Conf);
             //   _contentFeeder = new PlayableContentFeeder(this);
                _authLockEventWaitHandle.Set();
            }

            _receiver = new Receiver(this);


            _eventService.Language(_inner.PreferredLocale);
            TimeProvider.Init(this);

            Debug.WriteLine($"Authenticated as {_apWelcome.CanonicalUsername}!");
            Mercury().InterestedIn("spotify:user:attributes:update", this);
            Dealer().AddMessageListener(this, "hm://connect-state/v1/connect/logout");
        }

        /// <summary>
        /// Authenticates with the server. Does not create all the components unlike <see cref="Authenticate"/>.
        /// </summary>
        /// <param name="credentials"><see cref="Spotify.LoginCredentials"/></param>
        /// <param name="removeLock">Whether <see cref="_authLockEventWaitHandle"/> should be released or not.
        /// <code>false</code> for <see cref="Authenticate"/>
        /// <code>true</code> for <see cref="Reconnect"/></param>
        private void AuthenticatePartial(
            [NotNull] Spotify.LoginCredentials credentials,
            bool removeLock)
        {
            GuardAgainst.ArgumentBeingNull(_recvCipher);
            GuardAgainst.ArgumentBeingNull(_sendCipher);

            var clientResponseEncrypted = new ClientResponseEncrypted
            {
                LoginCredentials = credentials,
                SystemInfo = new SystemInfo
                {
                    Os = Os.Windows,
                    CpuFamily = CpuFamily.CpuX86,
                    SystemInformationString = "1",
                    DeviceId = _inner.DeviceId
                },
                VersionString = "1.0"
            };
            SendUnchecked(MercuryPacket.Type.Login, clientResponseEncrypted.ToByteArray(), _closedToken);

            var packet = Receive(_conn.Result.NetworkStream, _closedToken);
            switch (packet.Cmd)
            {
                case MercuryPacket.Type.APWelcome:
                {
                    _apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);
                    var bytes0X0F = new byte[20];
                    (new Random()).NextBytes(bytes0X0F);
                    SendUnchecked(MercuryPacket.Type.Unknown_0x0f, bytes0X0F, _closedToken);

                    using var preferredLocale = new MemoryStream(18 + 5);
                    preferredLocale.WriteByte((byte)0x0);
                    preferredLocale.WriteByte((byte)0x0);
                    preferredLocale.WriteByte((byte)0x10);
                    preferredLocale.WriteByte((byte)0x0);
                    preferredLocale.WriteByte((byte)0x02);
                    preferredLocale.Write("preferred-locale");
                    preferredLocale.Write(_inner.PreferredLocale);
                    SendUnchecked(MercuryPacket.Type.PreferredLocale, preferredLocale.ToArray(), _closedToken);
                    if (removeLock)
                    {
                        using (_authLock.Lock())
                        {
                            _authLockEventWaitHandle.Set();
                        }
                    }

                    try
                    {
                        if (_inner.Conf.StoreCredentials)
                        {
                            var jsonObj = new StoredCredentials
                            {
                                AuthenticationType = _apWelcome.ReusableAuthCredentialsType,
                                Base64Credentials = _apWelcome.ReusableAuthCredentials.ToBase64(),
                                Username = _apWelcome.CanonicalUsername
                            };
                            ApplicationData.Current.LocalSettings.Values["auth_data"] =
                                JsonConvert.SerializeObject(jsonObj);
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

        public Configuration Configuration() => _inner?.Conf;

        public DealerClient Dealer()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(_mercuryClient, exceptionMessage: "Session isn't authenticated");
            return _dealer;
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
            GuardAgainst.ArgumentBeingNull(_mercuryClient, exceptionMessage: "Session isn't authenticated");
            return _mercuryClient;
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
            if (_tokenProvider == null) throw new Exception("Session isn't authenticated!");
            return _tokenProvider;
        }

        public ApiClient Api()
        {
            WaitAuthLock();
            GuardAgainst.ArgumentBeingNull(_apiClient, exceptionMessage: "Session isn't authenticated");
            return _apiClient;
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
            GuardAgainst.ArgumentBeingNull(_eventService, exceptionMessage: "Session isn't authenticated");
            return _eventService;
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
            if (_closedToken.IsCancellationRequested)
            {
                Debug.WriteLine("Connection was broken while Session.close() has been called");
                return;
            }

            using(_authLock.Lock())
            {
                if (_sendCipher == null)
                {
                    try
                    {
                        _authLockEventWaitHandle.Wait(_closedToken);
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
            using (_sendLock.Lock(cts))
            {
                var a = _conn.Result.NetworkStream;
                var payloadLengthAsByte = BitConverter.GetBytes((short) payload.Length).Reverse().ToArray();
                var yetAnotherBuffer = new MemoryStream(3 + payload.Length);
                yetAnotherBuffer.WriteByte((byte) cmd);
                yetAnotherBuffer.Write(payloadLengthAsByte, 0, payloadLengthAsByte.Length);
                yetAnotherBuffer.Write(payload, 0, payload.Length);

                _sendCipher.nonce(Utils.toByteArray(_sendNonce));
                Interlocked.Increment(ref _sendNonce);

                var bufferBytes = yetAnotherBuffer.ToArray();
                _sendCipher.encrypt(bufferBytes);

                var fourBytesBuffer = new byte[4];
                _sendCipher.finish(fourBytesBuffer);
                a.Write(bufferBytes, 0, bufferBytes.Length);
                a.Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                a.Flush();
            }
        }


        internal MercuryPacket Receive(NetworkStream a, CancellationToken cts)
        {
            using (_recvLock.Lock(cts))
            {
                _recvCipher.nonce(Utils.toByteArray(_recvNonce));
                Interlocked.Increment(ref _recvNonce);

                var headerBytes = new byte[3];
                a.ReadComplete(headerBytes,0,headerBytes.Length);
                _recvCipher.decrypt(headerBytes);

                var cmd = headerBytes[0];
                short payloadLength = (short) ((headerBytes[1] << 8) | (headerBytes[2] & 0xFF));

                var payloadBytes = new byte[payloadLength];
                a.ReadComplete(payloadBytes, 0, payloadBytes.Length);
                _recvCipher.decrypt(payloadBytes);

                byte[] mac = new byte[4];
                a.ReadComplete(mac, 0, mac.Length);

                byte[] expectedMac = new byte[4];
                _recvCipher.finish(expectedMac);
                return new MercuryPacket((MercuryPacket.Type) cmd, payloadBytes);
            }
        }

        private void Reconnect()
        {
            lock (_reconnectionListeners)
            {
                foreach (var reconnectionListener in _reconnectionListeners)
                {
                    reconnectionListener.OnConnectionDropped();
                }
            }

            if (_conn != null)
            {
                _conn.Result.Conn.Close();
                _receiver.Stop();
            }

            try
            {
                _conn = SpotifyConnection.Create(_inner.Conf);
                Connect();
                AuthenticatePartial(new LoginCredentials
                {
                    Typ = _apWelcome.ReusableAuthCredentialsType,
                    Username = _apWelcome.CanonicalUsername,
                    AuthData = _apWelcome.ReusableAuthCredentials
                }, true);
                Debug.WriteLine($"Re-authenticated as {_apWelcome.CanonicalUsername}!");
                lock (_reconnectionListeners)
                {
                    foreach (var reconnectionListener in _reconnectionListeners)
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
            if (_closedToken.IsCancellationRequested)
            {
                Debug.WriteLine("Connection was broken while Session.close() has been called");
                return;
            }

            using (_authLock.Lock())
            {
                if (_sendCipher != null && _recvCipher != null) return;
                _authLockEventWaitHandle.Wait(_closedToken);
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
            private CancellationTokenSource _cts;
            private SpotifySession _session;

            internal Receiver(SpotifySession session)
            {
                _session = session;
                _cts = new CancellationTokenSource();
                var ts = new ThreadStart(BackgroundMethod);
                var backgroundThread = new Thread(ts);
                backgroundThread.Start();
            }

            public void Stop() => _cts.Cancel();
            private static void ParseProductInfo(byte[] @in)
            {
                Debug.WriteLine(Encoding.Default.GetString(@in));
            }

            private async void BackgroundMethod()
            {
                Debug.WriteLine("Session.Receiver started");
                while (!_cts.IsCancellationRequested)
                {
                    MercuryPacket packet;
                    MercuryPacket.Type cmd;
                    bool tokenCanceled = false;
                    try
                    {
                        try
                        {
                            packet = _session.Receive(_session._conn.Result.NetworkStream, _cts.Token);
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
                                        _session.Send(MercuryPacket.Type.Pong, packet.Payload);
                                    }
                                    catch (IOException ex)
                                    {
                                        Debug.WriteLine("Failed sending Pong!", ex);
                                    }
                                    break;
                                case MercuryPacket.Type.PongAck:
                                    break;
                                case MercuryPacket.Type.CountryCode:
                                    _session.CountryCode = Encoding.Default.GetString(packet.Payload);
                                    Debug.WriteLine("Received CountryCode: " + _session.CountryCode);
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
                                    _session.Mercury().Dispatch(packet);
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
                        if (!_cts.IsCancellationRequested)
                        {
                            Debug.WriteLine("Failed reading packet!" + ex.ToString());
                            _session.Reconnect();
                        }

                        break;
                    }

                    if (_cts.IsCancellationRequested)
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
