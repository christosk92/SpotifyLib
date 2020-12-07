using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Contexts
{
    public class GeneralFiniteContext : AbsSpotifyContext
    {
        public GeneralFiniteContext([NotNull] string context) : base(context)
        {
        }

        public override bool IsFinite() => true;
    }
}
