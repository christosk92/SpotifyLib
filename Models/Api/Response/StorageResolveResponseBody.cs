using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class StorageResolveResponseBody
    {
        [JsonProperty("cdnurl")]
        public IEnumerable<string> CdnUrls { get; set; }

   
        [JsonProperty("fileid")]
        public string FileId { get; set; }


        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
