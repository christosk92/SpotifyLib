using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Response
{
    public class AlbumsResponse
    {
        public List<FullAlbum> Albums { get; set; } = default!;
    }
}