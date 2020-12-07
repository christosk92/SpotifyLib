using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SpotifyLib.SpotifyConnect.Models
{
    public class CommandBody
    {
        public CommandBody([NotNull] JObject obj)
        {
            Obj = obj;
            Data = obj.ContainsKey("data")
                ? System.Convert.FromBase64String(obj["data"].ToString())
                : null;
            Value = obj.ContainsKey("value") ? obj["value"].ToString() : null;
        }

        public JObject Obj;
        public byte[] Data;
        public string Value;
    }
}