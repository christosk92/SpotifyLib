using System;
using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Contexts
{
    public abstract class AbsSpotifyContext
    {
        public readonly RestrictionsManager Restrictions;
        protected readonly string Context;

        protected AbsSpotifyContext([NotNull] string context)
        {
            Context = context;
            Restrictions = new RestrictionsManager(this);
        }
        public override string ToString()
        {
            return "AbsSpotifyContext{context='" + Context + "'}";
        }
        public static bool IsCollection(
            [NotNull] SpotifySession session,
            [NotNull] String uri) => uri.Equals("spotify:user:" + session.Username + ":collection");
        
        public static AbsSpotifyContext From([NotNull] string context)
        {
            if (context == null)
            {
                return new GeneralFiniteContext(context);
            }
            if (context.StartsWith("spotify:dailymix:") || context.StartsWith("spotify:station:"))
                return new GeneralInfiniteContext(context);
            else if (context.StartsWith("spotify:search:"))
                return new SearchContext(context, context.Substring(15));
            else
                return new GeneralFiniteContext(context);
        }
        public abstract bool IsFinite();
        public string Uri() => Context;
    }
}
