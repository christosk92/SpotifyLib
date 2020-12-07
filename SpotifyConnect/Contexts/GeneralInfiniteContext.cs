using System;
using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Contexts
{
    public class GeneralInfiniteContext : AbsSpotifyContext
    {
        public GeneralInfiniteContext([NotNull] String context) : base(context)
        {
        }

        public override bool IsFinite() => false;
    }
}