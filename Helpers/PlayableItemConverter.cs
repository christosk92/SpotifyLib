using System;
using System.Collections.Generic;
using System.Text;
using GuardAgainstLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLib.Models.Api.Response;

namespace SpotifyLib.Helpers
{
    public class PlayableItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType,
            object? existingValue, JsonSerializer serializer)
        {
            GuardAgainst.ArgumentBeingNull(serializer, nameof(serializer));

            var token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.Null)
            {
                return null;
            }

            var type = token["type"]?.Value<string>();
            switch (type)
            {
                case "track":
                {
                    var obj = new FullTrack();
                    serializer.Populate(token.CreateReader(), obj);
                    return obj;
                }
                case "episode":
                {
                    var obj = new FullEpisode();
                    serializer.Populate(token.CreateReader(), obj);
                    return obj;
                }
                default:
                    throw new Exception($"Received unkown playlist element type: {type}");
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
#nullable disable
    }
}