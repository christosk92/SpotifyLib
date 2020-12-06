using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Com.Spotify.Canvazcache;
using Google.Protobuf;
using SpotifyLib.Helpers;

namespace SpotifyLib.Api
{
    public class CanvazService
    {
        private ApResolver ap;
        public async Task<EntityCanvazResponse> GetCanvaz(string uri)
        {
            using var httpClient = new HttpClient();
            var entityCanvazRequest = new EntityCanvazRequest();
            entityCanvazRequest.Entities.Add(new Google.Protobuf.Collections.RepeatedField<EntityCanvazRequest.Types.Entity>
            {
                new EntityCanvazRequest.Types.Entity
                {
                    EntityUri = uri
                }
            });
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SpotifySession.Current.Tokens().GetToken("playlist-read").AccessToken);
            var resp = await httpClient.PostAsync($"{await ap.GetClosestSpClient()}/canvaz-cache/v0/canvases",
                new ByteArrayContent(entityCanvazRequest.ToByteArray()));
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadAsByteArrayAsync();
            return EntityCanvazResponse.Parser.ParseFrom(data);
        }
    }
}
