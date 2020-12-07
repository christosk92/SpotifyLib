using System;
using System.Collections.Generic;
using System.Linq;
using Connectstate;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using PlayOrigin = Spotify.Player.Proto.PlayOrigin;

namespace SpotifyLib.SpotifyConnect.Helpers
{
    public class ProtoUtils
    {
        public static List<ContextTrack> JsonToContextTracks([NotNull] JArray array)
        {
            return array.Select(JsonToContextTrack).ToList();
        }

        public static ContextTrack JsonToContextTrack([NotNull] JToken obj)
        {
            var j = obj["uri"];
            var i = obj["uid"];
            var b = new ContextTrack
            {
                Uri = obj["uri"]?.ToObject<string>() ?? "",
                Uid = obj["uid"]?.ToObject<string>() ?? ""
            };
            var z = obj["metadata"]?.ToList<JToken>().Select(x => new MapField<string, string>
            {
                {
                    x.ToObject<JProperty>()?.Name!, x.ToObject<JProperty>()?.Value.ToObject<string>()
                }
            });
            if (z == null) return b;
            foreach (var y in z)
                b.Metadata.Add(y);
            return b;
        }
        public static void EnrichTrack(
            [NotNull] ContextTrack subject, 
            [NotNull] ContextTrack track)
        {
            if (subject.HasUri && track.HasUri && !Object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            if (subject.HasGid && track.HasGid && !Object.Equals(subject.Gid, track.Gid))
                throw new Exception("Illegal Argument");

            foreach (var kvp in track.Metadata)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                subject.Metadata[key] = value;
            }
        }
        public static void EnrichTrack(
            [NotNull] ProvidedTrack subject,
            [NotNull] ContextTrack track)
        {
            if (track.HasUri && !object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            foreach (var kvp in track.Metadata)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                subject.Metadata[key] = value;
            }
        }

        public static Connectstate.PlayOrigin ConvertPlayOrigin([CanBeNull] PlayOrigin po)
        {
            if (po == null) return null;

            var builder = new Connectstate.PlayOrigin();

            if (!string.IsNullOrEmpty(po.FeatureIdentifier))
                builder.FeatureIdentifier = po.FeatureIdentifier;
            if (!string.IsNullOrEmpty(po.FeatureVersion))
                builder.FeatureVersion = po.FeatureVersion;
            if (!string.IsNullOrEmpty(po.ViewUri))
                builder.ViewUri = po.ViewUri;
            if (!string.IsNullOrEmpty(po.ExternalReferrer))
                builder.ExternalReferrer = po.ExternalReferrer;
            if (!string.IsNullOrEmpty(po.ReferrerIdentifier))
                builder.ReferrerIdentifier = po.ReferrerIdentifier;
            if (!string.IsNullOrEmpty(po.DeviceIdentifier))
                builder.DeviceIdentifier = po.DeviceIdentifier;

            builder.FeatureClasses.AddRange(po.FeatureClasses);
            return builder;
        }
        public static ContextPlayerOptions ConvertPlayerOptions(
            [CanBeNull] global::Spotify.Player.Proto.ContextPlayerOptions options)
        {
            if (options == null) return null;

            var builder = new ContextPlayerOptions();
            if (options.HasRepeatingContext)
                builder.RepeatingContext = options.RepeatingContext;
            if (options.HasRepeatingTrack)
                builder.RepeatingTrack = options.RepeatingTrack;
            if (options.HasShufflingContext)
                builder.ShufflingContext = options.ShufflingContext;
            return builder;
        }

        public static Context JsonToContext(JObject obj)
        {
            var c = new Context();
            if (obj.ContainsKey("uri"))
                c.Uri = obj["uri"]?.ToString();
            if (obj.ContainsKey("url"))
                c.Url = obj["url"]?.ToString();
         

            var metadata = obj["metadata"];
            if (metadata != null)
            {
                foreach (var key in metadata)
                {
                    c.Metadata.Add(key.Path, key.ToString());
                }
            }

            if (obj.ContainsKey("pages"))
            {
                foreach (var elm in obj["pages"]!)
                {
                    c.Pages.Add(JsonToContextPage(elm));
                }
            }

            return c;
        }
        public static ContextPlayerOptions JsonToPlayerOptions([NotNull] JObject obj,
            [CanBeNull] ContextPlayerOptions old)
        {
            old ??= new ContextPlayerOptions();
            if (obj != null)
            {
                if (obj.ContainsKey("repeating_context"))
                    old.RepeatingContext = obj["repeating_context"].ToObject<bool>();
                if (obj.ContainsKey("repeating_track"))
                    old.RepeatingTrack = obj["repeating_track"].ToObject<bool>();
                if (obj.ContainsKey("shuffling_context"))
                    old.ShufflingContext = obj["shuffling_context"].ToObject<bool>();
            }

            return old;
        }
        public static Connectstate.PlayOrigin JsonToPlayOrigin([NotNull] JObject obj)
        {
            var pl = new Connectstate.PlayOrigin();

            if(obj.ContainsKey("feauture_identifier")) 
                pl.FeatureIdentifier = obj["feauture_identifier"]?.ToString();
            if (obj.ContainsKey("feature_version"))
                pl.FeatureVersion = obj["feature_version"]?.ToString();
            if (obj.ContainsKey("view_uri"))
                pl.ViewUri = obj["view_uri"]?.ToString();
            if (obj.ContainsKey("external_referrer"))
                pl.ExternalReferrer = obj["external_referrer"]?.ToString();
            if (obj.ContainsKey("referrer_identifier"))
                pl.ReferrerIdentifier = obj["referrer_identifier"]?.ToString();
            if (obj.ContainsKey("device_identifier"))
                pl.DeviceIdentifier = obj["device_identifier"]?.ToString();
            return pl;
        }

        public static void CopyOverMetadata(
            [NotNull] Context from,
            [NotNull] PlayerState to)
        {
            foreach (var kvp in from.Metadata)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                to.ContextMetadata[key] = value;
            }
        }
        public static void CopyOverMetadata(
            [NotNull] ContextTrack from, 
            [NotNull] ContextTrack to)
        {
            foreach (var kvp in from.Metadata)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                to.Metadata[key] = value;
            }
        }
        public static void CopyOverMetadata([NotNull] ContextTrack from,
            [NotNull] ProvidedTrack to)
        {
            foreach (var kvp in from.Metadata)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                to.Metadata[key] = value;
            }
        }

        public static void CopyOverMetadata([NotNull] JObject obj, [NotNull] PlayerState to)
        {
            foreach (var kvp in obj)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                to.ContextMetadata[key] = value.ToObject<string>();
            }
        }

        public static ProvidedTrack ConvertToProvidedTrack([CanBeNull] ContextTrack track)
        {
            if (track == null) return null;

            var b = new ProvidedTrack();
            if (!string.IsNullOrEmpty(track.Uri))
                b.Uri = track.Uri;
            if (!string.IsNullOrEmpty(track.Uid))
                b.Uid = track.Uid;
            if (track.Metadata.ContainsKey("album_uri"))
                b.AlbumUri = track.Metadata["album_uri"];
            if (track.Metadata.ContainsKey("artist_uri"))
                b.ArtistUri = track.Metadata["artist_uri"];
            return b;
        }

        public static ContextPage JsonToContextPage([NotNull]JToken obj)
        {
            var b = new ContextPage();
            if (obj["next_page_url"] != null)
                b.NextPageUrl = obj["next_page_url"].ToObject<string>();
            if (obj["page_url"] != null)
                b.PageUrl = obj["page_url"].ToObject<string>();
            if (obj["tracks"] is JArray tracks)
            {
                b.Tracks.AddRange(JsonToContextTracks(tracks));
            }

            return b;
        }
        private static JObject MapToJson([NotNull] MapField<string, string> map)
        {
            var obj = new JObject();
            foreach (var kvp in map)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                obj[key] = value;
            }
            return obj;
        }

        public static JObject CraftContextStateCombo(
            [NotNull] PlayerState ps,
            [NotNull] List<ContextTrack> tracks)
        {
            var context = new JObject {["uri"] = ps.ContextUri, ["url"] = ps.ContextUrl};
            context.Add("metadata", MapToJson(ps.ContextMetadata));

            var pages = new JArray();
            context.Add("pages", pages);

            var page = new JObject {["page_url"] = "", ["next_page_url"] = ""};
            var tracksJson = new JArray();
            foreach (var t in tracks)
            {
                tracksJson.Add(TrackToJson(t));
            }
            page.Add("tracks", tracksJson);
            page.Add("metadata", MapToJson(ps.PageMetadata));
            pages.Add(page);


            var state = new JObject();

            var options = new JObject
            {
                ["shuffling_context"] = ps.Options.ShufflingContext,
                ["repeating_context"] = ps.Options.RepeatingContext,
                ["repeating_track"] = ps.Options.RepeatingTrack
            };

            state.Add("options", options);
            state.Add("skip_to", new JObject());
            state.Add("track", TrackToJson(ps.Track));

            var result = new JObject
            {
                {"context", context}, {"state", state}
            };
            return result;
        }


        public static List<ContextPage> JsonToContextPages([NotNull] JArray array) =>
            array.Select(JsonToContextPage).ToList();

        private static JObject TrackToJson([NotNull] ProvidedTrack track)
        {
            var obj = new JObject {["uri"] = track.Uri, ["uid"] = track.Uid};
            obj.Add("metadata", MapToJson(track.Metadata));
            return obj;
        }
        private static JObject TrackToJson([NotNull] ContextTrack track)
        {
            var obj = new JObject
            {
                {
                    "uri", track.Uri
                },
                {
                    "uid", track.Uid
                },
                {
                    "metadata", MapToJson(track.Metadata)
                }
            };
            return obj;
        }
    }
}
