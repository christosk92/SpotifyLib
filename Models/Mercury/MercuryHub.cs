using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace SpotifyLib.Models.Mercury
{
    public partial class MercuryHub
    {
        [J("root")] public Root Root { get; set; }
    }

    public partial class Root
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public RootModel Model { get; set; }
        [J("views")] public List<RootView> Views { get; set; }
    }

    public partial class RootModel
    {
        [J("logs")] public Logs Logs { get; set; }
        [J("tab")] public string Tab { get; set; }
        [J("tabs")] public List<Tab> Tabs { get; set; }
        [J("title")] public string Title { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }

    public partial class Logs
    {
        [J("impression")] public Impression Impression { get; set; }
    }

    public partial class Impression
    {
        [J("feature_id")] public string FeatureId { get; set; }
        [J("impression_type")] public string ImpressionType { get; set; }
        [J("request_id")] public string RequestId { get; set; }
        [J("section_id")] public string SectionId { get; set; }
    }

    public partial class Tab
    {
        [J("id")] public string Id { get; set; }
        [J("title")] public string Title { get; set; }
        [J("uri")] public string Uri { get; set; }
    }

    public partial class RootView
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public PurpleModel Model { get; set; }
        [J("views")] public List<ViewView> Views { get; set; }
    }

    public partial class PurpleModel
    {
        [J("id")] public string Id { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }

    public partial class ViewView
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public FluffyModel Model { get; set; }
        [J("views")] public List<object> Views { get; set; }
    }

    public partial class FluffyModel
    {
        [J("uri")] public string Uri { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }
}