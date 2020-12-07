using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using SpotifyLib.Mercury;
using SpotifyLib.SpotifyConnect.Helpers;

namespace SpotifyLib.SpotifyConnect.Spotify
{
    public class PagesLoader
    {
        private readonly List<ContextPage> _pages;
        private readonly SpotifySession _session;
        private String _resolveUrl = null;
        private int _currentPage = -1;

        public PagesLoader([NotNull] SpotifySession session)
        {
            this._session = session;
            this._pages = new List<ContextPage>();
        }


        public static PagesLoader From([NotNull] SpotifySession session, [NotNull] String context)
        {
            var loader = new PagesLoader(session) { _resolveUrl = context };
            return loader;
        }

        public static PagesLoader From([NotNull] SpotifySession session, [NotNull] Context context)
        {
            var pages = context.Pages.ToList();
            if (!pages.Any()) return From(session, context.Uri);

            var loader = new PagesLoader(session);
            loader._pages.AddRange(pages);
            return loader;
        }

        [NotNull]
        private List<ContextTrack>
            FetchTracks([NotNull] String url)
        {
            var resp = _session.Mercury().SendSync(new JsonMercuryRequest<string>(RawMercuryRequest.Get(url)));
            return ProtoUtils.JsonToContextTracks(JObject.Parse(resp)["tracks"] as JArray);
        }

        private List<ContextTrack> ResolvePage([NotNull] ContextPage page)
        {
            if (page.Tracks.Any())
            {
                return page.Tracks.ToList();
            }

            if (page.HasPageUrl)
            {
                return FetchTracks(page.PageUrl);
            }

            if (page.HasLoading && page.Loading)
            {
                throw new InvalidOperationException("What does loading even mean?");
            }

            throw new Exception("Cannot load page, not enough information!");
        }

        private List<ContextTrack> GetPage(int index)
        {
            if (index == -1) throw new Exception("You must call nextPage() first!");

            if (index == 0 && !_pages.Any() && _resolveUrl != null)
            {
                var m = _session.Mercury().SendSync(MercuryRequests.ResolveContext(_resolveUrl));
                var x = ProtoUtils.JsonToContextPages(JObject.Parse(m)["pages"] as JArray);
                _pages.AddRange(x);
            }

            _resolveUrl = null;

            if (index < _pages.Count)
            {
                var page = _pages[index];
                var tracks = ResolvePage(page);
                page.ClearPageUrl();
                page.Tracks.Clear();
                page.Tracks.Add(tracks);
                _pages[index] = page;
                return tracks;
            }
            else
            {
                if (index > _pages.Count) throw new IndexOutOfRangeException();

                var prev = _pages[index - 1];
                if (!prev.HasNextPageUrl) throw new Exception("Illegal State");

                var nextPageUrl = prev.NextPageUrl;
                prev.ClearNextPageUrl();
                _pages[index - 1] = prev;

                var tracks = FetchTracks(nextPageUrl);
                var tr = new ContextPage();
                tr.Tracks.AddRange(tracks);
                _pages.Add(tr);

                return tracks;
            }
        }

        public List<ContextTrack> CurrentPage()
        {
            return GetPage(_currentPage);
        }

        public bool NextPage()
        {
            try
            {
                GetPage(_currentPage + 1);
                _currentPage++;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void PutFirstPages([NotNull] List<ContextPage> pages)
        {
            if (_currentPage != -1 || this._pages.Any())
                this._pages.AddRange(pages);
        }

        public void PutFirstPage([NotNull] List<ContextTrack> tracks)
        {
            if (_currentPage != -1 || _pages.Any()) throw new Exception();
            var tr = new ContextPage();
            tr.Tracks.AddRange(tracks);
            _pages.Add(tr);
        }
    }
}
