using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Contexts
{
    public class SearchContext : GeneralFiniteContext
    {
        public readonly string SearchTerm;

        public SearchContext(
            [NotNull] string context,
            [NotNull] string searchTerm) : base(context)
        {
            SearchTerm = searchTerm;
        }
    }
}
