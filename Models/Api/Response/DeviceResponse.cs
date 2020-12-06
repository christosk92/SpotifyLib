using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Response
{
    public class DeviceResponse
    {
        public List<Device> Devices { get; set; } = default!;
    }
    public partial class Device
    {
        public string Id { get; set; } = default!;
        public bool IsActive { get; set; }
        public bool IsPrivateSession { get; set; }
        public bool IsRestricted { get; set; }
        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        [JsonProperty("volume_percent")]
        public int? VolumePercent { get; set; }
    }
}
