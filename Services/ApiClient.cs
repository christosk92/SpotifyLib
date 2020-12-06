using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Refit;

namespace SpotifyLib.Services
{
    public class ApiClient
    {
        private static ApResolver _apResolver;

        public static async Task<ApiClient> BuildApiClient([NotNull] SpotifySession session)
        {
            return new ApiClient()
            {
                Home = await CreateAndRegister<IHome>(session),
                Library = await CreateAndRegister<ILibrary>(session),
                User = await CreateAndRegister<IUserService>(session),
                Player = await CreateAndRegister<IPlayer>(session),
                PathFinder = await CreateAndRegister<IPathFinder>(session),
                Track = await CreateAndRegister<ITrack>(session),
                Playlist = await CreateAndRegister<IPlaylist>(session),
                PlaylistExtender = await CreateAndRegister<IPlaylistExtender>(session),
                StorageResolve = await CreateAndRegister<IStorageResolveService>(session),
                Metadata = await CreateAndRegister<IMetadata>(session),
                ConnectState = await CreateAndRegister<IConnectState>(session),
                Melody = await CreateAndRegister<IMelody>(session),
                Follow = await CreateAndRegister<IFollow>(session),
                Search = await CreateAndRegister<ISearch>(session),
                Artist = await CreateAndRegister<IArtist>(session),
                Album = await CreateAndRegister<IAlbum>(session),
                SeekTableService = await CreateAndRegister<ISeektables>(session),
                LicenseService = await CreateAndRegister<IPlaybackLicense>(session),
                CanvazService = new CanvazService()
        };
        }
        public IAlbum Album { get; private set; }
        public CanvazService CanvazService { get; private set; }
        public IPlaybackLicense LicenseService { get; private set; }
        public ISeektables SeekTableService { get; private set; }
        public IArtist Artist { get; private set; }
        public ISearch Search { get; private set; }
        public IMelody Melody { get; private set; }
        public IConnectState ConnectState { get; private set; }
        public IStorageResolveService StorageResolve { get; private set; }
        public IHome Home { get; private set; } 
        public ILibrary Library { get; private set; }
        public IFollow Follow { get; private set; }
        public IUserService User { get; private set; }
        public IPlayer Player { get; private set; }
        public IPathFinder PathFinder { get; private set; }
        public ITrack Track { get; private set; }
        public IMetadata Metadata { get; private set; }
        public IPlaylistExtender PlaylistExtender { get; private set; }
        public IPlaylist Playlist { get; private set; }
        private static async Task<T1> CreateAndRegister<T1>(
            SpotifySession session)
        {
            var type = typeof(T1);
            var baseUrl = new Uri(await ResolveBaseUrlFromAttribute(type));

            var c = new System.Net.Http.HttpClient(new AuthenticatedHttpClientHandler(()
                => session.Tokens().GetToken("playlist-read")?.AccessToken))
            {
                BaseAddress = baseUrl
            };
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            var createdService = RestService.For<T1>(c);
            ServiceLocator.Instance.Register(createdService);
            return createdService;
        }

        private static async Task<string> ResolveBaseUrlFromAttribute(MemberInfo type)
        {
            var attribute = Attribute.GetCustomAttributes(type);

            if (attribute.FirstOrDefault(x => x is BaseUrlAttribute) is BaseUrlAttribute baseUrlAttribute)
            {
                return baseUrlAttribute.BaseUrl;
            }

            if (attribute.Any(x => x is ResolvedDealerEndpoint))
            {
                return await _apResolver.GetClosestDealerAsync();
            }

            if (attribute.Any(x => x is ResolvedSpClientEndpoint))
            {
                return await _apResolver.GetClosestSpClient();
            }

            throw new NotImplementedException("No BaseUrl or ResolvedEndpoint attribute was defined");
        }
    }
}
